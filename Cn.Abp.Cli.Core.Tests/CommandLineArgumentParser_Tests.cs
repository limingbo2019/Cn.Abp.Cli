using Cn.Abp.Cli.Args;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Shouldly;
using System.Linq;

namespace Cn.Abp.Cli.Core.Tests
{
    public class CommandLineArgumentParser_Tests : AbpCliTestBase
    {
        private readonly ICommandLineArgumentParser _commandLineArgumentParser;
        public CommandLineArgumentParser_Tests()
        {
            _commandLineArgumentParser = GetRequiredService<ICommandLineArgumentParser>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new object[] { new string[0] })]
        public void Should_Parse_Empty_Arguments(string[] args)
        {
            var commandLineArgs = _commandLineArgumentParser.Parser(args);
            commandLineArgs.Command.ShouldBeNull();
            commandLineArgs.Target.ShouldBeNull();
            commandLineArgs.Options.Any().ShouldBeFalse();
        }
    }
}
