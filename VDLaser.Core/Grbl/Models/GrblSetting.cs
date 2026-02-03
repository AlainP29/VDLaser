namespace VDLaser.Core.Grbl.Models
{
    /// <summary>
    /// Représentation complète d’un paramètre GRBL ($0-$132, $140-$142).
    /// Utilisé pour : le parsing, le stockage et l’affichage.
    /// </summary>
    public class GrblSetting
    {
        /// <summary>ID numérique, ex: 100 pour $100</summary>
        public int Id { get; set; }

        /// <summary>
        /// Nom officiel GRBL : "$100", "$110", "$30"
        /// Injecté depuis GrblSettingCodes.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Description claire du setting</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Unité : "mm", "mm/min", "steps/mm"…</summary>
        public string Unit { get; set; } = string.Empty;
        /// <summary>Valeur du setting</summary>
        public double Value { get; set; }=0.0;

        /// <summary>Commentaire optionnel présent dans la ligne GRBL</summary>
        public string? Comment { get; set; }=string.Empty;

        public GrblSetting() { }

        public GrblSetting(int id, string name, string description, string unit = "")
        {
            Id = id;
            Name = name;
            Description = description;
            Unit = unit;
        }
        public GrblSetting(int id, string name, string description, string unit = "", double value=0.0, string comment="")
        {
            Id = id;
            Name = name;
            Description = description;
            Unit = unit;
            Value = value;
            Comment = comment;
        }
        public override string ToString()
        {
            string label = $"{Name} ({Unit})".Trim();
            return $"{label} = {Value}" + (Comment != null ? $" ({Comment})" : "");
        }
    }
}


