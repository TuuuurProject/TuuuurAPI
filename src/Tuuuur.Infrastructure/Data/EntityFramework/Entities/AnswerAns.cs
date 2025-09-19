using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class AnswerAns
{
    public int Id { get; set; }

    public int IdQuestion { get; set; }

    public string Value { get; set; }

    public bool Valid { get; set; }

    public virtual QuestionQst IdQuestionNavigation { get; set; }
}
