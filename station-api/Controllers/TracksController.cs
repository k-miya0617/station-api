#nullable disable

using Microsoft.AspNetCore.Mvc;
using station_api.Models;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Renci.SshNet;


namespace station_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly ILogger<TracksController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration _configuration;

        public TracksController(ILogger<TracksController> logger, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            this._logger = logger;
            this._hostingEnvironment = hostingEnvironment;
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
        /// TrackIDからファイルを取得する
        /// </summary>
        /// <param name="trackId">トラックID</param>
        /// <returns></returns>
        [HttpGet("{trackId}")]
        public IActionResult GetFile(string trackId)
        {
            // trackIdからファイルパスを取得する
            Track track = GetTrack(trackId);
            if (track == null) return NotFound();

            // TrackのLocationからファイル名を取得する
            string fileName = Path.GetFileName(track.Location);

            // TrackのLocationをLinuxのパス表記に変換する
            string replaceSourcePrefix = this._configuration.GetSection("NasSettings:ReplaceSourcePrefix").Value;
            string replaceToPrefix = this._configuration.GetSection("NasSettings:ReplaceToPrefix").Value;
            track.Location = track.Location.Replace(replaceSourcePrefix, replaceToPrefix);

            // NasへSCP接続し、ファイルを取得する
            try
            {
                // SCP接続情報を取得する
                string host = this._configuration.GetSection("NasSettings:Host").Value;
                string port = this._configuration.GetSection("NasSettings:Port").Value;
                string userName = this._configuration.GetSection("NasSettings:Username").Value;

                // 秘密鍵情報を取得する
                var _PrivateKey = new PrivateKeyAuthenticationMethod(userName, new PrivateKeyFile("/app/Ssh/id_rsa_homenas-station_root"));

                // 接続情報オブジェクトを生成する
                var connectionInfo = new Renci.SshNet.ConnectionInfo(
                    host,
                    Convert.ToInt32(port),
                    userName,
                    new AuthenticationMethod[]
                    {
                        _PrivateKey,
                    }
                );

                // ファイル保存用のメモリストリームを用意する
                MemoryStream ms = new MemoryStream();

                // 接続情報オブジェクトをもとにSSH接続する
                using (var client = new ScpClient(connectionInfo))
                {
                    client.RemotePathTransformation = RemotePathTransformation.ShellQuote;
                    client.Connect();

                    // 対象のファイルをダウンロードする
                    client.Download(track.Location, ms);
                }

                // ダウンロードしたファイルに基づいてヘッダーを作成する
                string fileNameUrl = System.Net.WebUtility.UrlEncode(fileName);

                // スペースが+に変換されるバグに対応。+を%20に置き換え
                fileNameUrl = fileNameUrl.Replace("+", "%20");
                Response.Headers.Append("Content-Disposition",$"attachment;filename=\"{fileNameUrl}\"");

                // 呼び出し元にファイルを送信する
                return File(ms.ToArray(), "application/download", fileName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 指定したトラックIDからTrackデータを取得する
        /// もし存在しない場合は、nullを返却する
        /// </summary>
        /// <param name="trackId">トラックID</param>
        /// <returns>指定したtrackIdのTrackデータ</returns>
        private Track GetTrack(string trackId)
        {
            Track track = null;

            // SQLコネクションの作成
            using (SqlConnection con = new SqlConnection(this.GenerateConnectionString()))
            {
                // コネクションのオープン
                con.Open();

                // SQLコマンドの作成
                SqlCommand cmd = con.CreateCommand();

                // SQLコマンドの作成
                StringBuilder sql = new StringBuilder();
                sql.Append(Track.TracksSelectSql());
                sql.AppendLine("where ");
                sql.AppendLine("    TrackID = @trackId ");

                cmd.CommandText = sql.ToString();

                List<SqlParameter> listPara = new List<SqlParameter>()
                {
                    new SqlParameter() { ParameterName = "@trackId", Value = trackId, SqlDbType = SqlDbType.NVarChar }
                };

                listPara.ForEach(param => cmd.Parameters.Add(param));

                // SQLの実行
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        track = new Track(reader);
                    }
                }

                con.Close();

                return track;
            }
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
                sql.AppendLine("order by Album, DiscNumber, TrackNumber ");

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

        public static string GenerateRandomString(int length = 16)
        {
            string charactors = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] charsArr = new char[length];
            Random random = new Random();

            for (int i = 0; i < charsArr.Length; i++)
                charsArr[i] = charactors[random.Next(charactors.Length)];

            return new string(charsArr);
        }
    }
}
