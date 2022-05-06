using System.Data.SqlClient;

namespace station_api
{
    /// <summary>
    /// 共通関数
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// SqlDataReaderからNullableなlong型のデータを取得する
        /// </summary>
        /// <param name="reader">SqlDataReader</param>
        /// <param name="keyName">キー名</param>
        /// <returns>long型の値、またはnull</returns>
        public static long? GetNullableLongValue(SqlDataReader reader, string keyName)
        {
            try
            {
                if (reader[keyName] == null) return null;
                return long.Parse(reader[keyName]?.ToString() ?? "");

            } catch (FormatException)
            {
                // Parseしようとした文字の書式が異常である場合, nullを返す
                return null;

            } catch (IndexOutOfRangeException)
            {
                // readerにkeyNameに該当するデータが含まれない場合、nullを返す
                return null;

            } catch (Exception)
            {
                // それ以外の例外の場合は、呼び出し元に処理させる
                throw;
            }
        }

        /// <summary>
        /// SqlDataReaderからNullableなint型のデータを取得する
        /// </summary>
        /// <param name="reader">SqlDataReader</param>
        /// <param name="keyName">キー名</param>
        /// <returns>int型の値、またはnull</returns>
        public static int? GetNullableIntValue(SqlDataReader reader, string keyName)
        {
            try
            {
                if (reader[keyName] == null) return null;
                return int.Parse(reader[keyName]?.ToString() ?? "");

            } catch (FormatException)
            {
                // Parseしようとした文字の書式が異常である場合, nullを返す
                return null;

            } catch (IndexOutOfRangeException)
            {
                // readerにkeyNameに該当するデータが含まれない場合、nullを返す
                return null;

            } catch (Exception)
            {
                // それ以外の例外の場合は、呼び出し元に処理させる
                throw;
            }
        }

        /// <summary>
        /// SqlDataReaderからNullableなDateTime型のデータを取得する
        /// </summary>
        /// <param name="reader">SqlDataReader</param>
        /// <param name="keyName">キー名</param>
        /// <returns>DateTime型の値、またはnull</returns>
        public static DateTime? GetNullableDateTimeValue(SqlDataReader reader, string keyName)
        {
            try
            {
                if (reader[keyName] == null) return null;
                return DateTime.Parse(reader[keyName]?.ToString() ?? "");
                
            } catch (FormatException)
            {
                // Parseしようとした文字の書式が異常である場合, nullを返す
                return null;

            } catch (IndexOutOfRangeException)
            {
                // readerにkeyNameに該当するデータが含まれない場合、nullを返す
                return null;

            } catch (Exception)
            {
                // それ以外の例外の場合は、呼び出し元に処理させる
                throw;
            }
        }

        /// <summary>
        /// SqlDataReaderからNullableなstring型のデータを取得する
        /// </summary>
        /// <param name="reader">SqlDataReader</param>
        /// <param name="keyName">キー名</param>
        /// <returns>string型のデータ、またはnull</returns>
        public static string? GetNullableStringValue(SqlDataReader reader, string keyName)
        {
            if (reader[keyName] == null) return null;
            return reader[keyName]?.ToString() ?? null;
        }
    }
}
