--CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE TABLE ocorrencias_motivos (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

  banco_id uuid NOT NULL REFERENCES bancos(id) ON DELETE CASCADE,

  ocorrencia varchar(10) NOT NULL,   -- ex: '02'
  motivo varchar(10) NOT NULL,       -- ex: '00'

  descricao text NOT NULL,           -- ex: 'Entrada confirmada'
  observacao text NULL,              -- ex: 'Significa que o banco aceitou...'

  ativo boolean NOT NULL DEFAULT true,
  criado_em timestamptz NOT NULL DEFAULT now(),
  atualizado_em timestamptz NULL,

  UNIQUE (banco_id, ocorrencia, motivo)
);

CREATE INDEX ix_ocm_banco_ocorrencia ON ocorrencias_motivos (banco_id, ocorrencia);
CREATE INDEX ix_ocm_banco_ocorrencia_motivo ON ocorrencias_motivos (banco_id, ocorrencia, motivo);
