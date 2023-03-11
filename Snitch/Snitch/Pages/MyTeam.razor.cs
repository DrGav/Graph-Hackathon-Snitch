using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Graph;
using Snitch.DTOs;

namespace Snitch.Pages
{
    public partial class MyTeam
    {
        [CascadingParameter]
        private Task<AuthenticationState>? authenticationStateTask { get; set; }

        private IList<User> allUsers = new List<User>();
        private IList<UserMonitorDTO> MonitoringUsers = new List<UserMonitorDTO>();

        private string status = string.Empty;
        private bool isError;

        protected override async Task OnInitializedAsync()
        {
            if (authenticationStateTask == null)
            {
                throw new AuthenticationException(new Error
                {
                    Message = "Unable to access authentication state"
                });
            }

            var graphClient = clientFactory.GetAuthenticatedClient();

            try
            {
                var users = await graphClient.Me.DirectReports.Request()
                                  .Select("displayName,mail,givenName,surname,id")
                                  .GetAsync();


                while (users.Count > 0)
                {
                    foreach (var u in users)
                    {
                        allUsers.Add((User)u);
                    }
                    if (users.NextPageRequest != null)
                    {
                        users = await users.NextPageRequest
                            .GetAsync();
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (var u in allUsers)
                {
                    var newUser = new UserMonitorDTO
                    {
                        UserId = u.Id,
                        DisplayName = u.DisplayName,
                        Email = u.Mail,
                        Monitoring = false,
                        AvailableMinutes = 0,
                        AwayMinutes = 0,
                        BusyMinutes = 0,
                        DoNotDisturbMinutes = 0
                    };

                    MonitoringUsers.Add(newUser);
                }

                StartMonitor();

            }
            catch (ServiceException exception)
            {
                isError = true;
                status = exception.Message;
            }
        }

        private void SetMonitor(string userId)
        {
            MonitoringUsers.Where(u => u.UserId == userId).FirstOrDefault().Monitoring = !MonitoringUsers.Where(u => u.UserId == userId).FirstOrDefault().Monitoring;
        }

        private async Task StartMonitor()
        {
            var graphClient = clientFactory.GetAuthenticatedClient();

            while (true)
            {
                foreach (var user in MonitoringUsers)
                {
                    if (user.Monitoring)
                    {
                        var presence = await graphClient.Users[user.UserId].Presence.Request().GetAsync();

                        switch (presence.Availability)
                        {
                            case "Available":
                                user.AvailableMinutes = user.AvailableMinutes + 1;
                                break;
                            case "Busy":
                                user.BusyMinutes = user.BusyMinutes + 1;
                                break;
                            case "Away":
                                user.AwayMinutes = user.AwayMinutes + 1;
                                break;
                            case "DoNotDisturb":
                                user.DoNotDisturbMinutes = user.DoNotDisturbMinutes + 1;
                                break;
                        }
                    }
                }

                await Task.Delay(5000);
                StateHasChanged();
            }
        }
    }
}
