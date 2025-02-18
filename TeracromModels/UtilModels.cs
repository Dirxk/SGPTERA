namespace TeracromModels
{
    public class RespuestaJson
    {
        public bool Resultado { get; set; } = false;
        public string Mensaje { get; set; } = string.Empty;
        public object Data { get; set; } = new { };
        public List<string> Errores { get; set; } = new List<string>();
    }
}
