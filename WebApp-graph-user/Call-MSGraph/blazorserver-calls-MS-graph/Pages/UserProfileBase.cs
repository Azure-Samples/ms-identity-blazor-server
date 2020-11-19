using Microsoft.AspNetCore.Components;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.Threading.Tasks;

namespace blazorserver_calls_MS_graph.Pages
{
    /// <summary>
    /// Base class for UserProfile component.
    /// Injects GraphServiceClient and calls Microsoft Graph /me endpoint.
    /// </summary>
    public class UserProfileBase : ComponentBase
    {
        [Inject]
        GraphServiceClient GraphClient { get; set; }
        [Inject] 
        MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; }

        protected User _user = new User();
        protected override async Task OnInitializedAsync()
        {
            await GetUserProfile();
        }

        /// <summary>
        /// Retrieves user information from Microsoft Graph /me endpoint.
        /// </summary>
        /// <returns></returns>
        private async Task GetUserProfile()
        {
            try
            {
                var request = GraphClient.Me.Request();
                _user = await request.GetAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ConsentHandler.HandleException(ex);
            }
        }
    }
}

