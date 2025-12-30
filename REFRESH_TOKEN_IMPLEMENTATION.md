# Configuration Refresh Token - Guide Frontend Vue.js

## 📋 Vue d'ensemble

Le système de refresh token permet à vos utilisateurs de rester connectés pendant **3 mois maximum** sans avoir à se reconnecter.

### Architecture

- **Access Token** : JWT valide pendant quelques minutes (configurable via `Validity` dans appsettings.json)
- **Refresh Token** : Token sécurisé valide pendant **90 jours** (3 mois)

---

## 🔧 Configuration Backend (appsettings.json)

Ajoutez ou mettez à jour la section `JwtSettings` dans vos fichiers `appsettings.json` :

```json
{
  "JwtSettings": {
    "Issuer": "votre-issuer",
    "Audience": "votre-audience",
    "Key": "votre-clé-secrète-super-longue-et-sécurisée",
    "Validity": 15,
    "RefreshTokenValidity": 90
  }
}
```

- **Validity** : Durée de vie de l'access token en **minutes** (ex: 15 minutes)
- **RefreshTokenValidity** : Durée de vie du refresh token en **jours** (90 jours = 3 mois)

---

## 🗄️ Migration Base de Données

La table `RefreshToken_RTK` a été créée. Pour l'appliquer à votre base de données, déployez le fichier SQL :

```bash
# Via SSDT ou exécutez directement le script
src/Tuuuur.Database/dbo/RefreshToken_RTK.sql
```

---

## 🚀 API Endpoints

### 1. Login / Register / Google Auth

Tous retournent maintenant un `refreshToken` en plus du token JWT :

**Réponse type** :

```json
{
  "token": {
    "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
    "validFrom": "2025-12-30T10:00:00Z",
    "validTo": "2025-12-30T10:15:00Z",
    "refreshToken": "base64-encoded-random-string",
    "refreshTokenExpiresAt": "2026-03-30T10:00:00Z"
  },
  "user": {
    "id": 1,
    "nickName": "john_doe",
    "email": "john@example.com",
    "isAdmin": false
  },
  "isGoogleUser": false
}
```

### 2. Nouveau Endpoint : Refresh Token

**URL** : `POST /api/v1/auth/refresh`

**Request Body** :

```json
{
  "refreshToken": "votre-refresh-token-stocké"
}
```

**Réponse** (200 OK) :

```json
{
  "token": {
    "token": "nouveau-jwt-access-token",
    "validFrom": "2025-12-30T11:00:00Z",
    "validTo": "2025-12-30T11:15:00Z",
    "refreshToken": "nouveau-refresh-token",
    "refreshTokenExpiresAt": "2026-03-30T11:00:00Z"
  },
  "user": { ... },
  "isGoogleUser": false
}
```

**Erreurs** (401 Unauthorized) :

```json
{
  "errors": [
    {
      "code": "AUTH_INVALID",
      "message": "Invalid refresh token" // ou "Refresh token has expired" / "Refresh token has been revoked"
    }
  ]
}
```

---

## 💻 Implémentation Frontend Vue.js

### 1. Service d'authentification (authService.js)

```javascript
// src/services/authService.js
import axios from "axios";

const API_URL = process.env.VUE_APP_API_URL || "https://votre-api.com/api/v1";

class AuthService {
  // Connexion
  async login(login, password) {
    const response = await axios.post(`${API_URL}/auth/login`, {
      login,
      password,
    });
    if (response.data.token) {
      this.saveTokens(response.data);
    }
    return response.data;
  }

  // Enregistrement après vérification 2FA
  async verifyAccount(login, code) {
    const response = await axios.post(`${API_URL}/auth/register`, {
      login,
      code,
    });
    if (response.data.token) {
      this.saveTokens(response.data);
    }
    return response.data;
  }

  // Connexion Google
  async googleLogin(googleToken) {
    const response = await axios.post(`${API_URL}/auth/google`, {
      token: googleToken,
    });
    if (response.data.token) {
      this.saveTokens(response.data);
    }
    return response.data;
  }

  // Refresh token
  async refreshToken() {
    const refreshToken = localStorage.getItem("refreshToken");
    if (!refreshToken) {
      throw new Error("No refresh token available");
    }

    try {
      const response = await axios.post(`${API_URL}/auth/refresh`, {
        refreshToken,
      });
      if (response.data.token) {
        this.saveTokens(response.data);
        return response.data.token.token;
      }
    } catch (error) {
      // Si le refresh token est invalide, déconnecter l'utilisateur
      this.logout();
      throw error;
    }
  }

  // Sauvegarder les tokens
  saveTokens(authData) {
    localStorage.setItem("accessToken", authData.token.token);
    localStorage.setItem("refreshToken", authData.token.refreshToken);
    localStorage.setItem("accessTokenExpiry", authData.token.validTo);
    localStorage.setItem(
      "refreshTokenExpiry",
      authData.token.refreshTokenExpiresAt
    );
    localStorage.setItem("user", JSON.stringify(authData.user));
  }

  // Récupérer l'access token
  getAccessToken() {
    return localStorage.getItem("accessToken");
  }

  // Vérifier si l'access token est expiré
  isAccessTokenExpired() {
    const expiry = localStorage.getItem("accessTokenExpiry");
    if (!expiry) return true;
    return new Date(expiry) <= new Date();
  }

  // Vérifier si le refresh token est expiré
  isRefreshTokenExpired() {
    const expiry = localStorage.getItem("refreshTokenExpiry");
    if (!expiry) return true;
    return new Date(expiry) <= new Date();
  }

  // Déconnexion
  logout() {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("accessTokenExpiry");
    localStorage.removeItem("refreshTokenExpiry");
    localStorage.removeItem("user");
  }

  // Obtenir l'utilisateur courant
  getCurrentUser() {
    const user = localStorage.getItem("user");
    return user ? JSON.parse(user) : null;
  }
}

export default new AuthService();
```

### 2. Intercepteur Axios (axiosInterceptor.js)

```javascript
// src/plugins/axiosInterceptor.js
import axios from "axios";
import authService from "@/services/authService";
import router from "@/router";

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Intercepteur de requête
axios.interceptors.request.use(
  (config) => {
    const token = authService.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Intercepteur de réponse
axios.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Si 401 et que ce n'est pas déjà une tentative de refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // Si un refresh est déjà en cours, mettre en file d'attente
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return axios(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      // Vérifier si le refresh token est valide
      if (authService.isRefreshTokenExpired()) {
        authService.logout();
        router.push("/login");
        return Promise.reject(error);
      }

      try {
        const newAccessToken = await authService.refreshToken();
        processQueue(null, newAccessToken);
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        return axios(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        authService.logout();
        router.push("/login");
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
```

### 3. Initialisation dans main.js

```javascript
// src/main.js
import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./plugins/axiosInterceptor"; // Importer l'intercepteur

const app = createApp(App);
app.use(router);
app.mount("#app");
```

### 4. Guard de navigation (router/index.js)

```javascript
// src/router/index.js
import { createRouter, createWebHistory } from "vue-router";
import authService from "@/services/authService";

const routes = [
  {
    path: "/login",
    name: "Login",
    component: () => import("@/views/Login.vue"),
  },
  {
    path: "/dashboard",
    name: "Dashboard",
    component: () => import("@/views/Dashboard.vue"),
    meta: { requiresAuth: true },
  },
  // ... autres routes
];

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes,
});

router.beforeEach(async (to, from, next) => {
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);

  if (requiresAuth) {
    const user = authService.getCurrentUser();

    if (!user) {
      // Pas d'utilisateur, rediriger vers login
      next("/login");
    } else if (authService.isRefreshTokenExpired()) {
      // Refresh token expiré, déconnecter
      authService.logout();
      next("/login");
    } else if (authService.isAccessTokenExpired()) {
      // Access token expiré, essayer de le rafraîchir
      try {
        await authService.refreshToken();
        next();
      } catch (error) {
        authService.logout();
        next("/login");
      }
    } else {
      // Tout est OK
      next();
    }
  } else {
    next();
  }
});

export default router;
```

### 5. Composant de connexion (Login.vue)

```vue
<template>
  <div class="login">
    <h1>Connexion</h1>
    <form @submit.prevent="handleLogin">
      <input
        v-model="login"
        type="text"
        placeholder="Email ou pseudo"
        required
      />
      <input
        v-model="password"
        type="password"
        placeholder="Mot de passe"
        required
      />
      <button type="submit">Se connecter</button>
    </form>
    <p v-if="error" class="error">{{ error }}</p>
  </div>
</template>

<script>
import authService from "@/services/authService";
import { useRouter } from "vue-router";
import { ref } from "vue";

export default {
  name: "Login",
  setup() {
    const router = useRouter();
    const login = ref("");
    const password = ref("");
    const error = ref("");

    const handleLogin = async () => {
      try {
        error.value = "";
        await authService.login(login.value, password.value);
        router.push("/dashboard");
      } catch (err) {
        error.value =
          err.response?.data?.errors?.[0]?.message || "Erreur de connexion";
      }
    };

    return {
      login,
      password,
      error,
      handleLogin,
    };
  },
};
</script>
```

---

## 🔐 Sécurité

### Bonnes pratiques implémentées :

1. **Refresh token révoqué après utilisation** : Chaque refresh génère un nouveau token
2. **Stockage sécurisé** : Les tokens sont stockés dans localStorage (pour une PWA, considérez IndexedDB)
3. **Expiration automatique** : Tokens expirés automatiquement rejetés
4. **Rotation des tokens** : Nouveau refresh token à chaque rafraîchissement

### Recommandations supplémentaires :

- **HTTPS uniquement** : N'utilisez jamais HTTP en production
- **HttpOnly Cookies** : Pour une sécurité maximale, envisagez de stocker le refresh token dans un cookie HttpOnly (nécessite des modifications backend)
- **Logout côté serveur** : Implémentez un endpoint `/auth/logout` pour révoquer tous les refresh tokens d'un utilisateur

---

## 📝 Configuration des durées

Pour ajuster la durée de connexion :

- **Access Token** : Modifiez `Validity` (en minutes) dans appsettings.json
  - Recommandé : 15-60 minutes
- **Refresh Token** : Modifiez `RefreshTokenValidity` (en jours) dans appsettings.json
  - Actuel : 90 jours (3 mois)
  - Pour 1 mois : `30`
  - Pour 6 mois : `180`
  - Pour 1 an : `365`

---

## 🧪 Tests

### Test manuel du refresh token :

```bash
# 1. Obtenir un refresh token via login
curl -X POST https://votre-api.com/api/v1/auth/login \\
  -H "Content-Type: application/json" \\
  -d '{"login": "test@example.com", "password": "password123"}'

# 2. Utiliser le refresh token
curl -X POST https://votre-api.com/api/v1/auth/refresh \\
  -H "Content-Type: application/json" \\
  -d '{"refreshToken": "le-refresh-token-reçu"}'
```

---

## 📚 Résumé des modifications

### Backend

✅ Table `RefreshToken_RTK` créée
✅ Modèle `RefreshToken` dans Domain
✅ Repository `IRefreshTokenRepository` et implémentation
✅ `JwtTokenResponse` mis à jour avec `RefreshToken` et `RefreshTokenExpiresAt`
✅ `IJwtFactory` avec méthode `GenerateRefreshToken()`
✅ `JwtFactory` génère et retourne les refresh tokens
✅ `JwtConfiguration` avec `RefreshTokenValidity`
✅ Endpoint `POST /auth/refresh` dans `AuthController`
✅ `RefreshTokenUseCase` pour gérer le renouvellement
✅ Use cases d'authentification mis à jour (Login, VerifyAccount, GoogleAuth)

### Frontend (À implémenter)

📋 Service d'authentification avec gestion des tokens
📋 Intercepteur Axios pour auto-refresh
📋 Guard de navigation pour vérifier l'authentification
📋 Stockage sécurisé dans localStorage

---

## 🎯 Prochaines étapes

1. **Déployer la base de données** : Exécutez le script SQL pour créer la table
2. **Configurer appsettings.json** : Ajoutez `RefreshTokenValidity: 90`
3. **Tester l'API** : Vérifiez que les endpoints retournent les refresh tokens
4. **Implémenter le frontend** : Suivez le code Vue.js ci-dessus
5. **Tester end-to-end** : Connexion → Utilisation → Auto-refresh → Déconnexion

---

Votre système de refresh token est maintenant prêt ! Les utilisateurs pourront rester connectés pendant 3 mois sans interruption. 🎉
