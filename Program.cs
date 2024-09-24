using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MediatRDemo
{
    public static class CustomMediatorExtensions
    {
        public static IServiceCollection AddCustomMediator(this IServiceCollection services)
        {
            services.AddMediatR(typeof(Program));
            services.AddSingleton<IMediator, CustomMediator>();
            return services;
        }
    }

    // Custom Mediator`  to invoke handlers by priority
    public class CustomMediator(ServiceFactory serviceFactory) : Mediator(serviceFactory)
    {
        private IPriorityDeleteHandler? GetPriorityDeleteHandler(Func<INotification, CancellationToken, Task> handler)
        {
            // TODO: we need some cleaner code like
            // handler.Target as IPriorityDeleteHandler, but how ?
            return (handler.Target?.GetType().GetFields()[0])?.GetValue(handler.Target) as IPriorityDeleteHandler;
        }

        protected override Task PublishCore(IEnumerable<Func<INotification, CancellationToken, Task>> allHandlers,
            INotification notification, CancellationToken cancellationToken)
        {
            // Order the handlers by their priority (smaller numbers first)
            var orderedHandlers = allHandlers
                .OrderBy(handler => GetPriorityDeleteHandler(handler)?.DeletePriority ?? int.MaxValue);

            foreach (var handler in orderedHandlers)
            {
                handler(notification, cancellationToken).Wait(cancellationToken); // Run synchronously, in order
            }

            return Task.CompletedTask;
        }
    }

    public interface IPriorityDeleteHandler
    {
        int? DeletePriority { get; }
    }

    // Notification: Defines a DeleteCase notification
    public class DeleteCase(int id) : INotification
    {
        public int Id => id;
    }

    // Notification: Defines a DeleteCaseFolder notification
    public class DeleteCaseFolder(int id) : INotification
    {
        public int Id => id;
    }

    class Cases(IMediator mediator)
    {
        private int Id = 10;

        public async Task Delete()
        {
            await mediator.Publish(new DeleteCase(Id));
            Console.WriteLine($"Deleting case {Id}");
        }
    }

    class CaseFolder(IMediator mediator) : INotificationHandler<DeleteCase>, IPriorityDeleteHandler
    {
        private int Id = 99;
        public int? DeletePriority => 5;

        public async Task DeleteBy(int caseId)
        {
            var t = Task.Run(() => Console.WriteLine($"Deleting case folder by case {caseId}"));
            await t;
        }

        public async Task Handle(DeleteCase notification, CancellationToken cancellationToken)
        {
            await mediator.Publish(new DeleteCaseFolder(Id), cancellationToken);
            await DeleteBy(notification.Id);
        }
    }

    class CaseDocuments(IMediator mediator) : INotificationHandler<DeleteCase>, IPriorityDeleteHandler
    {
        private int Id = 44;
        public int? DeletePriority => 2;

        public async Task DeleteBy(int caseId)
        {
            var t = Task.Run(() => Console.WriteLine($"Deleting case documents by case {caseId}"));
            await t;
        }

        public async Task Handle(DeleteCase notification, CancellationToken cancellationToken)
        {
            await DeleteBy(notification.Id);
        }
    }

    class CaseFolderNote : INotificationHandler<DeleteCaseFolder>, IPriorityDeleteHandler
    {
        private int Id = 88;
        public int? DeletePriority => 2;

        public async Task DeleteBy(int caseFolderId)
        {
            var t = Task.Run(() => Console.WriteLine($"Deleting folder notes by case folder {caseFolderId}"));
            await t;
        }

        public async Task Handle(DeleteCaseFolder notification, CancellationToken cancellationToken)
        {
            await DeleteBy(notification.Id);
        }
    }

    class CaseFolderImages : INotificationHandler<DeleteCaseFolder>, IPriorityDeleteHandler
    {
        private int Id = 77;
        public int? DeletePriority => 1;

        public async Task DeleteBy(int caseFolderId)
        {
            var t = Task.Run(() => Console.WriteLine($"Deleting folder images by case folder {caseFolderId}"));
            await t;
        }

        public async Task Handle(DeleteCaseFolder notification, CancellationToken cancellationToken)
        {
            await DeleteBy(notification.Id);
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // Setting up Dependency Injection
            var services = new ServiceCollection();

            // Register custom Mediator (overrides default publish strategy)
            services.AddCustomMediator();

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var cases = new Cases(mediator);
            await cases.Delete();
        }
    }
}