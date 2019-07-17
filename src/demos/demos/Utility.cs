using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <Summary>
/// This file contains some utilities commonly used.
/// </Summary>

namespace TFSCodeReviewer
{
    public class Utility
    {
        /// <summary>
        /// Get last substr split by specified separators.
        /// i.e. "http://www.baidu.com/news", split by '/', we will get string "news".
        /// </summary>
        /// <param name="original"> url or file path. </param>
        /// <param name="separators"> sepcified separators </param>
        /// <returns> last substr. </returns>
        public static string SplitOutLastSubstr(string original, char[] separators)
        {
            int lastSlashIndex = original.LastIndexOfAny(separators);
            if (lastSlashIndex == -1)
            {
                return original;
            }
            else if (lastSlashIndex == original.Length - 1)
            {
                return string.Empty;
            }

            return original.Substring(lastSlashIndex + 1);
        }

        /// <summary>
        /// Convert original's charset from srcCodec to dstCodec.
        /// </summary>
        /// <param name="srcCodec"> source codec. </param>
        /// <param name="dstCodec"> dst codec. </param>
        /// <param name="original"> original string. </param>
        /// <returns> converted string. </returns>
        public static string EncodingConvert(Encoding srcCodec, Encoding dstCodec, string original)
        {
            byte[] srcBytes = srcCodec.GetBytes(original);
            byte[] bytes = Encoding.Convert(srcCodec, dstCodec, srcBytes);

            return dstCodec.GetString(bytes);
        }
    }
}
