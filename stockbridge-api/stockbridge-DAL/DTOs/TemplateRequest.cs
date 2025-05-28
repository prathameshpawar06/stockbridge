namespace stockbridge_DAL.DTOs
{
    public class TemplateRequest
    {
        public ReqTemplatePrincipalModel? TemplatePrincipal { get; set; } = new ReqTemplatePrincipalModel();

        public List<ReqTemplateMajor>? TemplateMajor { get; set; } = new List<ReqTemplateMajor>();
        public List<int>? policies { get; set; }

    }

    public class ReqTemplatePrincipalModel
    {
        public int? PrincipalId { get; set; }

        public string? Name { get; set; } = null!;

        public string? Description { get; set; }

    }

    public class ReqTemplateMajor
    {
        public int? MajorId { get; set; }

        public int? PrincipalId { get; set; }

        public string? Name { get; set; } = null!;

        public string? Comments { get; set; }

        public int? Sequence { get; set; }

        public List<ReqTemplateMajorColDef>? TemplateMajorColDef { get; set; } = new List<ReqTemplateMajorColDef>();


    }

    public class ReqTemplateMajorColDef
    {
        public int? ColumnDefId { get; set; }

        public int? MajorId { get; set; }

        public int? Sequence { get; set; }

        public string? ColumnName { get; set; }

        public string? ColumnDescription { get; set; }

        public string? ColumnType { get; set; } = null!;

        public string? NumericSign { get; set; }

        public int? Width { get; set; }
        public List<ReqTemplateMinorDef>? TemplateMinorDefs { get; set; } = new List<ReqTemplateMinorDef>();

    }

    public class ReqTemplateMinorDef
    {
        public int? MinorId { get; set; }

        public int? ColumnDefId { get; set; }

        public int? RowSequence { get; set; }

        public string? ColumnValue { get; set; } = null!;

        public int? DefinitionId { get; set; }
    }

}
