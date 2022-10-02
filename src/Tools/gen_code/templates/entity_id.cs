using DDD.Domain;

{{ namespace }}
{
    public class {{ class_name }} : EntityId
    {
        public {{ class_name }}(DomainModelVersion domainModelVersion, string value) : base(domainModelVersion, value) { }

        public static {{ class_name }} Create(string value)
            => Create(DomainModelVersion.Latest(), value);

        public static {{ class_name }} Create(DomainModelVersion domainModelVersion, string value)
        {
            var {{ obj_var_name }} = new {{ class_name }}(domainModelVersion, value);
            {{ obj_var_name }}.Validate(nameof({{ obj_var_name }}));
            return {{ obj_var_name }};
        }
    }
}
