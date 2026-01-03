namespace RemessaSeguraBakend.Models {
    public class OcorrenciaMotivo {
        public Guid Id { get; set; }
        public Guid BancoId { get; set; }
        public string Ocorrencia { get; set; } = "";
        public string Motivo { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string? Observacao { get; set; }


    }
}
