using System.Data.SqlClient;
using System.Text;

namespace station_api.Models
{
    public class Track
    {
        public long TrackID { get; set; }
        public long? SizeByte { get; set; }
        public long? TotalTimeMs { get; set; }
        public int? DiscNumber { get; set; }
        public int? DiscCount { get; set; }
        public int? TrackNumber { get; set; }
        public int? TrackCount { get; set; }
        public int? Year { get; set; }
        public int? Bpm { get; set; }
        public int? ArtworkCount { get; set; }
        public string? Name { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? AlbumArtist { get; set; }
        public string? Composer { get; set; }
        public string? Genre { get; set; }
        public int? PlayCount { get; set; }
        public string? Kind { get; set; }
        public string? Location { get; set; }
        public DateTime? DateAdded { get; set; }
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// SqlDataReaderからTrackオブジェクトを生成する
        /// </summary>
        /// <param name="reader">Tracksテーブルを取得したSqlDataReader</param>
        public Track(SqlDataReader reader)
        {
            this.TrackID = Common.GetNullableLongValue(reader, "TrackID") ?? 0;
            this.SizeByte = Common.GetNullableLongValue(reader, "SizeByte");
            this.TotalTimeMs = Common.GetNullableLongValue(reader, "TotalTimeMs");
            this.DiscNumber = Common.GetNullableIntValue(reader, "DiscNumber");
            this.DiscCount = Common.GetNullableIntValue(reader, "DiscCount");
            this.TrackNumber = Common.GetNullableIntValue(reader, "TrackNumber");
            this.TrackCount = Common.GetNullableIntValue(reader, "TrackCount");
            this.Year = Common.GetNullableIntValue(reader, "Year");
            this.Bpm = Common.GetNullableIntValue(reader, "Bpm");
            this.ArtworkCount = Common.GetNullableIntValue(reader, "ArtworkCount");
            this.Name = Common.GetNullableStringValue(reader, "Name");
            this.Artist = Common.GetNullableStringValue(reader, "Artist");
            this.Album = Common.GetNullableStringValue(reader, "Album");
            this.AlbumArtist = Common.GetNullableStringValue(reader, "AlbumArtist");
            this.Composer = Common.GetNullableStringValue(reader, "Composer");
            this.Genre = Common.GetNullableStringValue(reader, "Genre");
            this.PlayCount = Common.GetNullableIntValue(reader, "PlayCount");
            this.Kind = Common.GetNullableStringValue(reader, "Kind");
            this.Location = Common.GetNullableStringValue(reader, "Location");
            this.DateAdded = Common.GetNullableDateTimeValue(reader, "DateAdded");
            this.DateModified = Common.GetNullableDateTimeValue(reader, "DateModified");
        }

        /// <summary>
        /// Tracksテーブルを取得するSQL文
        /// </summary>
        /// <returns></returns>
        public static string TracksSelectSql()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select");
            sql.AppendLine("	 TrackID ");
            sql.AppendLine("	,SizeByte ");
            sql.AppendLine("	,TotalTimeMs ");
            sql.AppendLine("	,DiscNumber ");
            sql.AppendLine("	,DiscCount ");
            sql.AppendLine("	,TrackNumber ");
            sql.AppendLine("	,TrackCount ");
            sql.AppendLine("	,Year ");
            sql.AppendLine("	,Bpm ");
            sql.AppendLine("	,ArtworkCount ");
            sql.AppendLine("	,Name ");
            sql.AppendLine("	,Artist ");
            sql.AppendLine("	,Album ");
            sql.AppendLine("	,AlbumArtist ");
            sql.AppendLine("	,Composer ");
            sql.AppendLine("	,Genre ");
            sql.AppendLine("	,PlayCount ");
            sql.AppendLine("	,Kind ");
            sql.AppendLine("	,Location ");
            sql.AppendLine("	,DateAdded ");
            sql.AppendLine("	,DateModified ");
            sql.AppendLine("from Tracks ");
            return sql.ToString();
        }
    }
}
