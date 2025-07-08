namespace BancoDigitalAna.Shared.Helpers
{
    public static class Constantes
    {
        // Tipos de Erros
        public const string INTERNAL_ERROR = "INTERNAL_ERROR";
        public const string INVALID_DOCUMENT = "INVALID_DOCUMENT";
        public const string USER_UNAUTHORIZED = "USER_UNAUTHORIZED";
        public const string INVALID_ACCOUNT = "INVALID_ACCOUNT";
        public const string INACTIVE_ACCOUNT = "INACTIVE_ACCOUNT";
        public const string INVALID_VALUE = "INVALID_VALUE";
        public const string INVALID_TYPE = "INVALID_TYPE";

        // Tipos de Movimentos
        public enum TIPOS_MOVIMENTOS
        { 
            D, // Débito
            C  // Crédito
        };
    }
}
