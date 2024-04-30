using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CalculatorCsharp
{
    public interface IOperationProvider
    {
        IEnumerable<Operation> Get();
    }

    public class OperationProvider : IOperationProvider
    {
        private IEnumerable<Operation> operations;

        public OperationProvider(IEnumerable<Operation> operations)
        {
            this.operations = operations;
        }

        public IEnumerable<Operation> Get()
        {
            return operations;
        }
    }

    public class Application
    {
        private IWindsorContainer container;
        private IOperationProvider operationProvider;
        private IMenu menu;
        private IEnumerable<Operation> operations;

        public Application(
            IWindsorContainer container,
            IOperationProvider operationProvider,
            IMenu menu)
        {
            this.container = container;
            this.operationProvider = operationProvider;
            this.menu = menu;
        }

        public void Run()
        {
            operations = operationProvider.Get();
            Operation selectedOperation = menu.ShowAndGetOperation(operations.ToArray());
            double result = selectedOperation.Run(10, 5);
            Console.WriteLine(result);
        }
    }

    public class Program
    {
        private static IWindsorContainer _container = new WindsorContainer();

        public static void Main()
        {
            try
            {
                Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Start()
        {
            _container.AddFacility<StartableFacility>(f => f.DeferredStart());
            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel));
            _container.Install(new LocalInstaller());

            // Create Application instance and run
            var application = _container.Resolve<Application>();
            application.Run();
        }
    }

    internal class LocalInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IWindsorContainer>().Instance(container),
                Component.For<Application>()
                         .StartUsingMethod("Run"),

                Component.For<IMenu>()
                         .ImplementedBy<NewMenu>()
                         .LifestyleTransient(),

                Component.For<IOperationProvider>()
                         .ImplementedBy<OperationProvider>(),

                Component.For<Operation>()
                         .ImplementedBy<Addition>(),
                Component.For<Operation>()
                         .ImplementedBy<Substraction>(),
                Component.For<Operation>()
                         .ImplementedBy<Multiplacation>(),
                Component.For<Operation>()
                         .ImplementedBy<Division>(),
                Component.For<Operation>()
                         .ImplementedBy<Sqrt>()
            );
        }
    }

    public interface IMenu
    {
        Operation ShowAndGetOperation(Operation[] operations);
    }

    public sealed class Menu : IMenu
    {
        public Operation ShowAndGetOperation(Operation[] operations)
        {
            DisplayMenu(operations);
            return GetUserChoice(operations);
        }

        private void DisplayMenu(Operation[] operations)
        {
            Console.WriteLine("======== КАЛЬКУЛЯТОР ========");
            for (int i = 0; i < operations.Length; i++)
            {
                Operation operation = operations[i];
                Console.WriteLine($"{i + 1}. ОПЕРАЦИЯ {operation.Name};");
            }
        }

        private Operation GetUserChoice(Operation[] operations)
        {
            Console.Write("Выберите действие: ");
            int userInput = Convert.ToInt32(Console.ReadLine());
            // Check if the user input is within the valid range
            if (userInput >= 1 && userInput <= operations.Length)
            {
                return operations[userInput - 1];
            }
            else
            {
                // If the user input is invalid, return null or a default object
                return null;
            }
        }
    }

    public sealed class NewMenu : IMenu
    {
        public Operation ShowAndGetOperation(Operation[] operations)
        {
            DisplayMenu(operations);
            return GetUserChoice(operations);
        }

        private void DisplayMenu(Operation[] operations)
        {
            Console.WriteLine("....КАЛЬКУЛЯТОР....");
            for (int i = 0; i < operations.Length; i++)
            {
                Operation operation = operations[i];
                Console.WriteLine($"{i + 1}. {operation.Name};");
            }
        }

        private Operation GetUserChoice(Operation[] operations)
        {
            Console.Write("Выберите действие: ");
            int userInput = Convert.ToInt32(Console.ReadLine());
            // Check if the user input is within the valid range
            if (userInput >= 1 && userInput <= operations.Length)
            {
                return operations[userInput - 1];
            }
            else
            {
                // If the user input is invalid, return null or a default object
                return null;
            }
        }
    }

    public abstract class Operation
    {
        public abstract string Name { get; }

        public abstract double Run(params double[] numbers);
    }

    public sealed class Addition : Operation
    {
        public override string Name => "Сложение";

        public override double Run(params double[] numbers)
        {
            return numbers.Sum();
        }
    }

    public sealed class Substraction : Operation
    {
        public override string Name => "Вычитание";

        public override double Run(params double[] numbers)
        {
            double result = numbers[0];
            for (int i = 1; i < numbers.Length; i++)
            {
                result -= numbers[i];
            }
            return result;
        }
    }

    public sealed class Multiplacation : Operation
    {
        public override string Name => "Умножение";

        public override double Run(params double[] numbers)
        {
            return numbers.Aggregate(1.0, (current, number) => current * number);
        }
    }

    public sealed class Division : Operation
    {
        public override string Name => "Деление";

        public override double Run(params double[] numbers)
        {
            if (numbers.Length == 0)
                throw new ArgumentException("At least one number must be provided.");

            double result = numbers[0];
            foreach (var number in numbers.Skip(1))
            {
                if (number == 0)
                    throw new DivideByZeroException("Cannot divide by zero.");
                result /= number;
            }
            return result;
        }
    }

    public sealed class Sqrt : Operation
    {
        public override string Name => "Квадратный корень";

        public override double Run(params double[] numbers)
        {
            if (numbers.Length != 1)
                throw new ArgumentException("Exactly one number must be provided for square root operation.");

            double number = numbers[0];
            if (number < 0)
                throw new ArgumentException("Square root is undefined for negative numbers.");

            return Math.Sqrt(number);
        }
    }
}
