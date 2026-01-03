CREATE TABLE bancos (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  numero_banco int NOT NULL UNIQUE,
  nome text NOT NULL
);

-- banco
INSERT INTO bancos (numero_banco, nome)
VALUES (237, 'Bradesco')
ON CONFLICT (numero_banco) DO NOTHING;

-- ocorrência 02
WITH b AS (
  SELECT id FROM bancos WHERE numero_banco = 237
)
INSERT INTO ocorrencias_bancarias (banco_id, codigo_ocorrencia, descricao)
SELECT b.id, '02', 'Entrada confirmada'
FROM b
ON CONFLICT (banco_id, codigo_ocorrencia) DO NOTHING;

-- motivo 00 da ocorrência 02
WITH o AS (
  SELECT o.id
  FROM ocorrencias_bancarias o
  JOIN bancos b ON b.id = o.banco_id
  WHERE b.numero_banco = 237 AND o.codigo_ocorrencia = '02'
)
INSERT INTO motivos_ocorrencia (ocorrencia_id, codigo_motivo, descricao)
SELECT o.id, '00', 'Entrada confirmada'
FROM o
ON CONFLICT (ocorrencia_id, codigo_motivo) DO NOTHING;