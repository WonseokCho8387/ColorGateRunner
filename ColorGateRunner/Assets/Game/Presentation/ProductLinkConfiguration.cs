using System;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public enum ProductLinkType
    {
        Terms = 0,
        Privacy = 1,
        Support = 2
    }

    [CreateAssetMenu(
        fileName = "ProductLinkConfiguration",
        menuName = "Color Gate Runner/Product Link Configuration")]
    public sealed class ProductLinkConfiguration : ScriptableObject
    {
        [SerializeField] private string termsUrl = string.Empty;
        [SerializeField] private string privacyUrl = string.Empty;
        [SerializeField] private string supportUrl = string.Empty;

        internal bool TryGetHttpsUrl(
            ProductLinkType type,
            out string url)
        {
            string candidate = type == ProductLinkType.Terms
                ? termsUrl
                : type == ProductLinkType.Privacy
                    ? privacyUrl
                    : supportUrl;
            if (Uri.TryCreate(
                    candidate,
                    UriKind.Absolute,
                    out Uri parsed) &&
                string.Equals(
                    parsed.Scheme,
                    Uri.UriSchemeHttps,
                    StringComparison.OrdinalIgnoreCase))
            {
                url = parsed.AbsoluteUri;
                return true;
            }

            url = string.Empty;
            return false;
        }

        internal void ConfigureForTests(
            string terms,
            string privacy,
            string support)
        {
            termsUrl = terms ?? string.Empty;
            privacyUrl = privacy ?? string.Empty;
            supportUrl = support ?? string.Empty;
        }
    }

    internal interface IExternalUrlOpener
    {
        bool TryOpen(string url, out string error);
    }

    internal sealed class UnityExternalUrlOpener : IExternalUrlOpener
    {
        public bool TryOpen(string url, out string error)
        {
            try
            {
                Application.OpenURL(url);
                error = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }
    }
}
