namespace RemessaSeguraBakend.Models {
    public class Noticia {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Resumo { get; set; } = "";
        public string Conteudo { get; set; } = "";
        public string? ImagemCapa { get; set; }
        public string? Categoria { get; set; }
        public string? Status { get; set; }
        public DateTime? DataPublicacao { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Publicado { get; set; }

        public string? AutorNome { get; set; }
        public int Visualizacoes { get; set; }
        public bool Destaque { get; set; }
        public string? MetaDescription { get; set; }
        public int? OrdemDestaque { get; set; }

        // Fonte
        public string? FonteNome { get; set; }
        public string? FonteUrl { get; set; }
        public DateTime? FontePublicadaEm { get; set; }
        public string? FonteAutor { get; set; }
    }
}
