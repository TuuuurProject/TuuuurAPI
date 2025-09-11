using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class Project : IEntity
{ }

public partial class User : IEntity
{ }

public partial class CodeFramework : IEntity
{ }

public partial class Language : IEntity
{ }

public partial class Localize : IEntity
{ }

public partial class LocalizeLanguages : IEntity
{ }

public partial class LocalizeTranslateGroups : IEntity
{ }

public partial class ProjectLanguages : IEntity
{ }

public partial class ProjectUsers : IEntity
{ }

public partial class ProjectTranslateGroups : IEntity
{ }

public partial class TranslateGroup : IEntity
{ }