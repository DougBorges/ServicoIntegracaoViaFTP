INSERT INTO ParametrosSistema (Cd_Parametro, De_Parametro, Vr_Parametro, Dt_Atualizacao)
VALUES ('EXPORTAR_ZIP', 'Exportar ou não os arquivos compactados em .zip', '1', SYSDATE);

INSERT INTO Configuracao_FTP (Sq_Sequencial, Nm_Endereco, Nu_Porta, Nm_Usuario, Ds_Senha, Ds_Recorrencia, Nu_DiaRecorrencia, Nu_HoraProgramada, Dt_Atualizacao)
VALUES (1, 'ftp://192.168.0.2/', '21', 'ftpteste', 'a12#b45', 'M', '1', '06:00', 'OS1280', SYSDATE);

CREATE OR REPLACE VIEW vw_Produtos
AS (SELECT Cd_Produto,
           Ds_Produto,
	       Id_EmpresaFabricante,
           Nu_PrecoCompra,
           Nu_PrecoVenda
      FROM Produto
     WHERE Id_Enviar = 1)
ORDER BY Cd_Produto;

CREATE OR REPLACE VIEW vw_Empresas
AS (SELECT E.Cd_Empresa,
           TE.Ds_TipoEmpresa,
           E.Ds_Nome,
           LE.Nm_Logradouro,
           LE.Nu_Numero,
           LE.Ds_Complemento,
           LE.Nm_Bairro,
           LE.Nm_Municipio,
           LE.Nm_Uf,
           TRIM (TO_CHAR (LE.Nu_Cep, '00000000')),
           LE.Nu_Telefone_Primario,
           LE.Nu_Telefone_Secundario,
           E.Ds_EMail,
           E.Ds_SiteUrl,
           E.Cnpj,
           E.Ds_RazaoSocial,
           E.Nm_ResponsavelTecnico,
           E.Ds_Acessibilidade,
           CASE WHEN E.Atendimento24Horas IS NOT NULL THEN 'Sim' ELSE 'Não' END,
           E.Ds_Observacoes
      FROM Empresa E
           INNER JOIN Localidade_Empresa LE ON (E.Cd_Empresa = LE.Cd_Empresa)
           INNER JOIN Tipo_Empresa TE ON (E.Cd_TipoEmpresa = TE.Cd_TipoEmpresa)
     WHERE TE.Cd_TipoEmpresa IN ('C', 'F', 'H')
           AND LE.Cd_TipoEndereco IN ('D','M')
           AND TE.Id_Enviar = 1)
ORDER BY Cd_Empresa;

CREATE OR REPLACE VIEW vw_TiposEmpresa
AS (SELECT Cd_TipoEmpresa,
           Ds_TipoEmpresa
      FROM Tipo_Empresa)
ORDER BY Cd_TipoEmpresa;

COMMIT;