using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class QuestionQst
{
    public int Id { get; set; }

    public string Question { get; set; }

    public int IdDifficulty { get; set; }

    public virtual ICollection<AnswerAns> AnswerAns { get; set; } = new List<AnswerAns>();

    public virtual DifficultyDft IdDifficultyNavigation { get; set; }

    public virtual ICollection<PartyQuestionPqt> PartyQuestionPqt { get; set; } = new List<PartyQuestionPqt>();

    public virtual ICollection<QuestionThemeQth> QuestionThemeQth { get; set; } = new List<QuestionThemeQth>();
}
