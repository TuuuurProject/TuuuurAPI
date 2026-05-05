using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class QuestionThemeQth
{
    public int Id { get; set; }

    public int IdQuestion { get; set; }

    public int IdTheme { get; set; }

    public virtual QuestionQst IdQuestionNavigation { get; set; }

    public virtual ThemeThm IdThemeNavigation { get; set; }
}
