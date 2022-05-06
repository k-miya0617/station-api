#nullable disable

using Microsoft.AspNetCore.Mvc;
using station_api.Models;
using System.Data;
using System.Data.SqlClient;
using System.Text;


namespace station_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly ILogger<TracksController> _logger;
        public IConfiguration _configuration;

        public TracksController(ILogger<TracksController> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;
        }

        /// <summary>
        /// appsettings.jsonからSQL接続文字列を生成する
        /// </summary>
        /// <returns>SQL接続文字列</returns>
        private string GenerateConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder()
            {
                DataSource = this._configuration.GetSection("DbSettings:DataSource").Value,
                IntegratedSecurity = false,
                UserID = this._configuration.GetSection("DbSettings:UserID").Value,
                Password = this._configuration.GetSection("DbSettings:Password").Value,
                InitialCatalog = this._configuration.GetSection("DbSettings:InitialCatalog").Value,
            };

            return builder.ToString();
        }

        /// <summary>
        /// アルバムに登録されているトラックの一覧を取得する
        /// </summary>
        /// <param name="album"></param>
        /// <returns></returns>
        [HttpGet("TracksInAlbum")]
        public ActionResult<List<Track>> TracksInAlbum(string album)
        {
            List<Track> list = new List<Track>();

            // SQLコネクションの作成
            using (SqlConnection con = new SqlConnection(this.GenerateConnectionString()))
            {
                // コネクションのオープン
                con.Open();

                // SQLコマンドの作成
                SqlCommand cmd = con.CreateCommand();

                // SQL文の作成
                StringBuilder sql = new StringBuilder();
                sql.Append(Track.TracksSelectSql());
                sql.AppendLine("where ");
                sql.AppendLine("	Album = @album ");
                sql.AppendLine("order by ");
                sql.AppendLine("DiscNumber, TrackNumber ");

                cmd.CommandText = sql.ToString();

                List<SqlParameter> listPara = new List<SqlParameter>()
                {
                    new SqlParameter() { ParameterName = "@album", Value = album, SqlDbType = SqlDbType.NVarChar}
                };

                listPara.ForEach(param => cmd.Parameters.Add(param));

                // SQLの実行
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Track(reader));
                    }
                }

                con.Close();

                if (list.Count == 0) return NotFound();
                return Ok(list);
            }
        }

        /// <summary>
        /// DBに登録されているアルバムの一覧を取得する
        /// </summary>
        /// <returns></returns>
        [HttpGet("AlbumList")]
        public ActionResult<List<string>> AlbumList()
        {
            List<string> list = new List<string>();

            // SQLコネクションの作成
            using (SqlConnection con = new SqlConnection(this.GenerateConnectionString()))
            {
                // コネクションのオープン
                con.Open();

                // SQLコマンドの作成
                SqlCommand cmd = con.CreateCommand();

                // SQL文の作成
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select ");
                sql.AppendLine("	Album ");
                sql.AppendLine("from ");
                sql.AppendLine("	Tracks ");
                sql.AppendLine("group by Album ");
                sql.AppendLine("having Album is not null ");

                cmd.CommandText = sql.ToString();

                // SQLの実行
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(Common.GetNullableStringValue(reader, "Album"));
                    }
                }

                con.Close();

                if (list.Count == 0)
                {
                    return NotFound();
                }

                return Ok(list);

            }
        }


        /// <summary>
        /// 検索キーワードからトラックを取得する
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        /// <param name="sortKey">ソート順の列名 (任意)</param>
        /// <returns></returns>
        [HttpGet("FindKeyword")]
        public ActionResult<List<Track>> FindKeyword(string keyword)
        {
            List<Track> list = new List<Track>();

            // SQLコネクションの作成
            using (SqlConnection con = new SqlConnection(this.GenerateConnectionString()))
            {
                // コネクションのオープン
                con.Open();

                // SQLコマンドの作成
                SqlCommand cmd = con.CreateCommand();

                // SQL文の作成
                StringBuilder sql = new StringBuilder();
                sql.Append(Track.TracksSelectSql());
                sql.AppendLine("where Name like @keyword ");
                sql.AppendLine("or Artist like @keyword ");
                sql.AppendLine("or Album like @keyword ");
                sql.AppendLine("or AlbumArtist like @keyword ");
                sql.AppendLine("or Composer like @keyword ");

                cmd.CommandText = sql.ToString();

                // パラメータの追加
                List<SqlParameter> sqlParams = new List<SqlParameter>()
                {
                    new SqlParameter() { ParameterName = "@keyword", Value = $"%{keyword}%", SqlDbType = SqlDbType.NVarChar},
                };
                sqlParams.ForEach(param => cmd.Parameters.Add(param));

                // SQLの実行
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Track(reader));
                    }
                }

                con.Close();

                // 戻り値
                if (list.Count == 0) return NotFound();
                return Ok(list);
            }
        }
    }
}
