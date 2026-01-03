CREATE TABLE noticias (
    id UUID PRIMARY KEY,
    titulo VARCHAR(250) NOT NULL,
    slug VARCHAR(250) NOT NULL UNIQUE,
    resumo TEXT,
    conteudo TEXT NOT NULL,
    publicado BOOLEAN NOT NULL DEFAULT FALSE,
    data_publicacao TIMESTAMP,
    autor_id UUID NOT NULL,
    criado_em TIMESTAMP NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMP,

    CONSTRAINT fk_noticias_autor
        FOREIGN KEY (autor_id)
        REFERENCES usuarios(id)
);

/*portal rápido*/
CREATE INDEX idx_noticias_publicado
ON noticias (publicado);

CREATE INDEX idx_noticias_data_publicacao
ON noticias (data_publicacao DESC);
