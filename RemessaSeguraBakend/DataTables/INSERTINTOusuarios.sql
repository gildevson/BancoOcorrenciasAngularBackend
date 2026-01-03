/*-- EXTENSÃO (para uuid)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =========================
-- 1) USUÁRIOS
-- =========================
DROP TABLE IF EXISTS usuario_permissoes;
DROP TABLE IF EXISTS permissoes;
DROP TABLE IF EXISTS usuarios;

CREATE TABLE usuarios (
  id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  nome         varchar(120) NOT NULL,
  email        varchar(180) NOT NULL UNIQUE,
  senha_hash   text NOT NULL,
  ativo        boolean NOT NULL DEFAULT true,
  criado_em    timestamptz NOT NULL DEFAULT now()
);

-- =========================
-- 2) PERMISSÕES (TIPOS)
-- =========================
CREATE TABLE permissoes (
  id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  codigo     varchar(40) NOT NULL UNIQUE,  -- ADMIN | PORTAL | SUPERVISOR
  descricao  varchar(120) NOT NULL,
  criado_em  timestamptz NOT NULL DEFAULT now()
);

-- =========================
-- 3) USUARIO x PERMISSÕES
-- =========================
CREATE TABLE usuario_permissoes (
  usuario_id   uuid NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
  permissao_id uuid NOT NULL REFERENCES permissoes(id) ON DELETE RESTRICT,
  criado_em    timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (usuario_id, permissao_id)
);

-- =========================
-- SEED das 3 permissões
-- =========================
INSERT INTO permissoes (codigo, descricao)
VALUES
  ('ADMIN', 'Administrador do sistema'),
  ('PORTAL', 'Usuário comum do portal'),
  ('SUPERVISOR', 'Usuário supervisor')
ON CONFLICT (codigo) DO NOTHING;
