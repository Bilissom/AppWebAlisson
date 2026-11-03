using AppWebAlisson.Configs;
using AppWebAlisson.Model;
using MySql.Data.MySqlClient;

namespace AppWebAlisson.DAO
{
    public class ProcessoDAO
    {
        private readonly Conexao _conexao;

        // A Conexao é injetada pelo container de dependências
        public ProcessoDAO(Conexao conexao)
        {
            _conexao = conexao;
        }

        public List<Processo> Listar()
        {
            var lista = new List<Processo>();

            var comando = _conexao.CreateCommand("SELECT * FROM processos;");
            var leitor = (MySqlDataReader)comando.ExecuteReader();

            while (leitor.Read())
            {
                lista.Add(MapearProcesso(leitor));
            }

            return lista;
        }

        // CREATE — insere um novo processo no banco
        public void Inserir(Processo processo)
        {
            try
            {
                string sql = @"INSERT INTO processos
                               (numero_pro, data_pro, interessado_pro,
                                assunto_pro, descricao_pro, situacao_pro)
                               VALUES
                               (@numero, @data, @interessado, @assunto, @descricao, @situacao)";
                
                using var comando = _conexao.CreateCommand(sql);
                comando.Parameters.AddWithValue("@numero", processo.Numero);
                comando.Parameters.AddWithValue("@data", processo.Data!.Value.ToDateTime(TimeOnly.MinValue));
                comando.Parameters.AddWithValue("@interessado", processo.Interessado);
                comando.Parameters.AddWithValue("@assunto", processo.Assunto);
                comando.Parameters.AddWithValue("@descricao", processo.Descricao);
                comando.Parameters.AddWithValue("@situacao", processo.Situacao);
                
                comando.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
        }

        public Processo? ObterPorId(int id)
        {
            using var con = _conexao.GetConnection();
            const string sql = @"SELECT id_pro, numero_pro, data_pro, interessado_pro,
                                        assunto_pro, descricao_pro, situacao_pro
                                 FROM processos
                                 WHERE id_pro = @id";

            using var comando = con.CreateCommand();
            comando.CommandText = sql;
            comando.Parameters.AddWithValue("@id", id);

            using var leitor = comando.ExecuteReader();
            if (!leitor.Read())
            {
                return null;
            }

            return new Processo
            {
                Id = leitor.GetInt32("id_pro"),
                Numero = leitor.GetString("numero_pro"),
                Data = DateOnly.FromDateTime(leitor.GetDateTime("data_pro")),
                Interessado = leitor.GetString("interessado_pro"),
                Assunto = leitor.GetString("assunto_pro"),
                Descricao = DAOHelper.GetString(leitor, "descricao_pro"),
                Situacao = leitor.GetString("situacao_pro")
            };
        }

        public void Atualizar(Processo processo)
        {
            using var con = _conexao.GetConnection();
            const string sql = @"UPDATE processos
                                 SET numero_pro = @numero,
                                     data_pro = @data,
                                     interessado_pro = @interessado,
                                     assunto_pro = @assunto,
                                     descricao_pro = @descricao,
                                     situacao_pro = @situacao
                                 WHERE id_pro = @id";

            using var comando = con.CreateCommand();
            comando.CommandText = sql;
            comando.Parameters.AddWithValue("@numero", processo.Numero);
            comando.Parameters.AddWithValue("@data", processo.Data!.Value.ToDateTime(TimeOnly.MinValue));
            comando.Parameters.AddWithValue("@interessado", processo.Interessado);
            comando.Parameters.AddWithValue("@assunto", processo.Assunto);
            comando.Parameters.AddWithValue("@descricao", processo.Descricao);
            comando.Parameters.AddWithValue("@situacao", processo.Situacao);
            comando.Parameters.AddWithValue("@id", processo.Id);

            comando.ExecuteNonQuery();
        }

        public void Excluir(int id)
        {
            using var con = _conexao.GetConnection();
            const string sql = "DELETE FROM processos WHERE id_pro = @id";

            using var comando = con.CreateCommand();
            comando.CommandText = sql;
            comando.Parameters.AddWithValue("@id", id);

            comando.ExecuteNonQuery();
        }

        // Método auxiliar: converte a linha atual do leitor em um objeto Processo.
        // Usa o DAOHelper para ler com segurança as colunas que podem ser NULL.
        private static Processo MapearProcesso(MySqlDataReader leitor)
        {
            var dt = DAOHelper.GetDateTime(leitor, "data_pro");
            return new Processo
            {
                Id = leitor.GetInt32("id_pro"),
                Numero = DAOHelper.GetString(leitor, "numero_pro"),
                Data = dt.HasValue ? DateOnly.FromDateTime(dt.Value) : null,
                Interessado = DAOHelper.GetString(leitor, "interessado_pro"),
                Assunto = DAOHelper.GetString(leitor, "assunto_pro"),
                Descricao = DAOHelper.GetString(leitor, "descricao_pro"),
                Situacao = DAOHelper.GetString(leitor, "situacao_pro")
            };
        }
    }
}
