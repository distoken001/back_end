using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Util;

namespace deMarketService.Common.Common
{

    public class EthereumSignatureVerifier
    {

        private static string MESSAGE = "I saw the strong tiger jump onto a tree on a sunny day.";
        /**
        * 对签名消息，原始消息，账号地址三项信息进行认证，判断签名是否有效
        * @param signature
        * @param message
        * @param address
        * @return
        */
        public static bool Verify(string signature, string address)
        {
            var signer = new EthereumMessageSigner();
            var messageBytes = Encoding.UTF8.GetBytes(MESSAGE);
            //var messageHash = new Sha3Keccack().CalculateHash(messageBytes).ToHex();

            //var signatureBytes = signature.HexToByteArray();
            var publicKey = signer.EcRecover(messageBytes, signature);
            //var recoveredAddress = new EthECKey(publicKey).GetPublicAddress();

            //return recoveredAddress.Equals(address, StringComparison.OrdinalIgnoreCase);
            return publicKey.Equals(address, StringComparison.OrdinalIgnoreCase);
        }
    }

}


