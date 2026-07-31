using System;
using ColorGateRunner.Product;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class UnityJsonSaveDocumentSerializer :
        ISaveDocumentSerializer
    {
        public string Serialize(LocalSaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return JsonUtility.ToJson(data, true);
        }

        public bool TryDeserialize(
            string serialized,
            out LocalSaveData data,
            out string diagnostic)
        {
            data = null;
            diagnostic = string.Empty;
            if (string.IsNullOrWhiteSpace(serialized))
            {
                diagnostic = "The save document is empty.";
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<LocalSaveData>(serialized);
                if (data == null)
                {
                    diagnostic = "The save root could not be decoded.";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                diagnostic = exception.Message;
                return false;
            }
        }
    }
}
