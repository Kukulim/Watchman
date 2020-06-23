﻿using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services.Factories;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devscord.DiscordFramework.Commons.Extensions;
using Watchman.Cqrs;
using Watchman.DomainModel.DiscordServer.Queries;
using Watchman.Discord.Areas.Users.Services;
using Devscord.DiscordFramework.Framework.Commands.Responses;
using Devscord.DiscordFramework.Commons.Exceptions;
using Watchman.Discord.Areas.Commons;
using Watchman.Discord.Areas.Users.BotCommands;

namespace Watchman.Discord.Areas.Users.Controllers
{
    public class UsersController : IController
    {
        private readonly IQueryBus _queryBus;
        private readonly MessagesServiceFactory _messagesServiceFactory;
        private readonly RolesService _rolesService;

        public UsersController(IQueryBus queryBus, MessagesServiceFactory messagesServiceFactory, RolesService rolesService)
        {
            this._queryBus = queryBus;
            this._messagesServiceFactory = messagesServiceFactory;
            this._rolesService = rolesService;
        }

        public void GetAvatar(AvatarCommand avatarCommand, Contexts contexts)
        {
            var messageService = this._messagesServiceFactory.Create(contexts);
            if (string.IsNullOrEmpty(contexts.User.AvatarUrl))
            {
                messageService.SendResponse(x => x.UserDoesntHaveAvatar(contexts.User));
                return;
            }

            messageService.SendMessage(contexts.User.AvatarUrl);
        }

        public void AddRole(AddRoleCommand addRoleCommand, Contexts contexts)
        {
            if (!addRoleCommand.Roles.Any())
            {
                throw new NotEnoughArgumentsException();
            }
            var safeRoles = this._queryBus.Execute(new GetDiscordServerSafeRolesQuery(contexts.Server.Id)).SafeRoles;
            var messageService = this._messagesServiceFactory.Create(contexts);
            this._rolesService.AddRoleToUser(safeRoles, messageService, contexts, addRoleCommand.Roles);
        }

        [DiscordCommand("remove role")] //todo
        public void RemoveRole(DiscordRequest request, Contexts contexts)
        {
            var commandRole = request.OriginalMessage.Replace("-remove role ", string.Empty); //TODO use DiscordRequest properties
            var safeRoles = this._queryBus.Execute(new GetDiscordServerSafeRolesQuery(contexts.Server.Id)).SafeRoles;
            var messagesService = this._messagesServiceFactory.Create(contexts);
            this._rolesService.DeleteRoleFromUser(safeRoles, messagesService, contexts, commandRole);
        }

        public async Task PrintRoles(RolesCommand rolesCommand, Contexts contexts)
        {
            var messageService = this._messagesServiceFactory.Create(contexts);
            var query = new GetDiscordServerSafeRolesQuery(contexts.Server.Id);
            var safeRoles = this._queryBus.Execute(query).SafeRoles.ToList();

            if (safeRoles.Count == 0)
            {
                await messageService.SendResponse(x => x.ServerDoesntHaveAnySafeRoles());
                return;
            }

            var output = new StringBuilder();
            output.PrintManyLines(safeRoles.Select(x => x.Name).ToArray(), contentStyleBox: false, spacesBetweenLines: false);
            await messageService.SendResponse(x => x.AvailableSafeRoles(output.ToString()));
        }
    }
}
