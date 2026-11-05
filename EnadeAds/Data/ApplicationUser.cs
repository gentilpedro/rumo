using Microsoft.AspNetCore.Identity;

namespace EnadeAds.Data;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string FullName { get; set; } = "";

    /// <summary>Quando a pessoa aceitou os termos de uso e a política de privacidade, e qual versão.</summary>
    public DateTimeOffset? TermsAcceptedAt { get; set; }

    public string? TermsVersion { get; set; }

    /// <summary>
    /// Declarou ter de 13 a 17 anos no cadastro. Junto vem <see cref="GuardianConsentAt"/>: quando declarou que o
    /// responsável legal leu os termos e autorizou o cadastro (LGPD, art. 14).
    /// </summary>
    [PersonalData]
    public bool IsMinor { get; set; }

    public DateTimeOffset? GuardianConsentAt { get; set; }

    /// <summary>Quantas questões a pessoa quer responder por dia (uma de <see cref="Achievements.XpRules.DailyGoalOptions"/>).</summary>
    public int DailyGoal { get; set; } = Achievements.XpRules.DefaultDailyGoal;
}
