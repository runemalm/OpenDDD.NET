using OpenDDD.Domain.Model.BuildingBlocks.Entity;

{{ namespace }}
{
    public class {{ class_name }} : EntityId
    {
        public {{ class_name }}(string value) : base(value) { }

        public static {{ class_name }} Create(string value)
        {
            var {{ obj_var_name }} = new {{ class_name }}(value);
            {{ obj_var_name }}.Validate(nameof({{ obj_var_name }}));
            return {{ obj_var_name }};
        }
    }
}
