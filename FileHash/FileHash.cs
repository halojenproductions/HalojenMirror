using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HalojenBackups.FileHash {
	internal static class FileHash {
		//private static MD5 hashingAlg;
		internal static async Task<byte[]> GetHash(FileInfo fileInfo) {
			using (FileStream sourceFS = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read)) {
				using (MD5 hashingAlg = MD5.Create()) {
					// Compute file hashes
					return await hashingAlg.ComputeHashAsync(sourceFS);
				}
			}
		}
	}
}
