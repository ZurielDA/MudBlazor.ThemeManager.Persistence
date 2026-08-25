using SAMACDX.ThemeManager.Persistence.Entities.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Persistence.Seeders.Themes
{
    public static class ThemeTermsSeeder
    {
        private static readonly ThemeTerm[] DefaultTerms =
        [
            // ── Actores del dominio ──────────────────────────────────────────────────
            new ThemeTerm { Key = "Citizen",       Singular = "Ciudadano",       Plural = "Ciudadanos",       Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Taxpayer",      Singular = "Contribuyente",   Plural = "Contribuyentes",   Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "PublicOfficial",Singular = "Funcionario",     Plural = "Funcionarios",     Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Auditor",       Singular = "Auditor",         Plural = "Auditores",        Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Supervisor",    Singular = "Supervisor",      Plural = "Supervisores",     Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Authority",     Singular = "Autoridad",       Plural = "Autoridades",      Gender = "Feminine",  Special = "" },

            // ── Entidades organizacionales ───────────────────────────────────────────
            new ThemeTerm { Key = "Department",       Singular = "Dependencia",                 Plural = "Dependencias",                  Gender = "Feminine",  Special = "" },
            new ThemeTerm { Key = "FiscalEntity",     Singular = "Entidad Fiscalizada",         Plural = "Entidades Fiscalizadas",         Gender = "Feminine",  Special = "" },
            new ThemeTerm { Key = "FiscalEntityGroup",Singular = "Grupo de Entidades",          Plural = "Grupos de Entidades",            Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "FiscalEntityType", Singular = "Tipo de Entidad Fiscalizada", Plural = "Tipos de Entidad Fiscalizada",   Gender = "Masculine", Special = "" },

            // ── Procesos y obligaciones ──────────────────────────────────────────────
            new ThemeTerm { Key = "Procedure",       Singular = "Trámite",             Plural = "Trámites",              Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "FiscalObligation",Singular = "Obligación Fiscal",   Plural = "Obligaciones Fiscales", Gender = "Feminine",  Special = "" },
            new ThemeTerm { Key = "Process",         Singular = "Proceso",             Plural = "Procesos",              Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "ProcessType",     Singular = "Tipo de Obligación",  Plural = "Tipos de Obligación",   Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Periodicity",     Singular = "Periodicidad",        Plural = "Periodicidades",        Gender = "Feminine",  Special = "" },
            new ThemeTerm { Key = "PeriodicityType", Singular = "Tipo de Periodicidad",Plural = "Tipos de Periodicidad", Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Period",          Singular = "Periodo",             Plural = "Periodos",              Gender = "Masculine", Special = "" },

            // ── Auditoría ────────────────────────────────────────────────────────────
            new ThemeTerm { Key = "Audit",          Singular = "Auditoría",              Plural = "Auditorías",               Gender = "Feminine",  Special = "" },
            new ThemeTerm { Key = "AuditCatalog",   Singular = "Catálogo de Auditoría",  Plural = "Catálogos de Auditoría",   Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "AuditAssignment",Singular = "Asignación",             Plural = "Asignaciones",             Gender = "Feminine",  Special = "" },

            // ── Documentos ───────────────────────────────────────────────────────────
            new ThemeTerm { Key = "File",              Singular = "Expediente",           Plural = "Expedientes",           Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Document",          Singular = "Documento",            Plural = "Documentos",            Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "RequiredDocument",  Singular = "Documento Requerido",  Plural = "Documentos Requeridos", Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "DocumentType",      Singular = "Tipo de Documento",    Plural = "Tipos de Documento",    Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "SignatureDocument",  Singular = "Documento de Firma",   Plural = "Documentos de Firma",   Gender = "Masculine", Special = "" },

            // ── Comunicación y notificaciones ────────────────────────────────────────
            new ThemeTerm { Key = "Notification", Singular = "Notificación",       Plural = "Notificaciones",       Gender = "Feminine",  Special = "" },
            new ThemeTerm { Key = "Announcement", Singular = "Comunicado",         Plural = "Comunicados",          Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "EmailTemplate",Singular = "Plantilla de Correo",Plural = "Plantillas de Correo", Gender = "Feminine",  Special = "" },
            new ThemeTerm { Key = "Message",      Singular = "Mensaje",            Plural = "Mensajes",             Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Conversation", Singular = "Conversación",       Plural = "Conversaciones",       Gender = "Feminine",  Special = "" },

            // ── Gestión y catálogos ──────────────────────────────────────────────────
            new ThemeTerm { Key = "Catalog",      Singular = "Catálogo",          Plural = "Catálogos",           Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Policy",       Singular = "Política",          Plural = "Políticas",           Gender = "Feminine",  Special = "" },
            new ThemeTerm { Key = "Resolution",   Singular = "Resolución",        Plural = "Resoluciones",        Gender = "Feminine",  Special = "" },
            new ThemeTerm { Key = "Report",       Singular = "Informe",           Plural = "Informes",            Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Program",      Singular = "Programa",          Plural = "Programas",           Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "LegalContent", Singular = "Contenido Legal",   Plural = "Contenidos Legales",  Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Status",       Singular = "Estado",            Plural = "Estados",             Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Dashboard",    Singular = "Panel de Control",  Plural = "Paneles de Control",  Gender = "Masculine", Special = "" },
            new ThemeTerm { Key = "Address",      Singular = "Dirección",         Plural = "Direcciones",         Gender = "Feminine",  Special = "" },
        ];

        public static async Task SeedAsync(DbContext context)
        {
            var existingKeys = context.Set<ThemeTerm>()
                .Select(t => t.Key)
                .ToHashSet();

            var newTerms = DefaultTerms
                .Where(t => !existingKeys.Contains(t.Key))
                .ToArray();

            if (newTerms.Length == 0)
                return;

            await context.Set<ThemeTerm>().AddRangeAsync(newTerms);
            await context.SaveChangesAsync();
        }
    }
}
