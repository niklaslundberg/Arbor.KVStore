namespace Arbor.KVStore.Web
{
    public static class RouteConstants
    {
        public const string ImportRoute = "~/import";

        public const string ImportRouteName = nameof(ImportRoute);

        public const string StartRoute = "~/";

        public const string StartRouteName = nameof(StartRoute);

        public const string DeleteRoute = "~/delete";

        public const string DeleteRouteName = nameof(DeleteRoute);

        public const string ClientValuesRoute = "~/{clientId}/values";

        public const string ClientValuesRouteName = nameof(ClientValuesRoute);

        public const string ClientRoute = "~/client";

        public const string ClientRouteName = nameof(ClientRoute);
    }
}