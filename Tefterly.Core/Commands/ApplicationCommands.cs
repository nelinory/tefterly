﻿using Prism.Commands;

namespace Tefterly.Core.Commands
{
    public class ApplicationCommands : IApplicationCommands
    {
        private readonly CompositeCommand _navigateCommand = new CompositeCommand();
        public CompositeCommand NavigateCommand
        {
            get { return _navigateCommand; }
        }
    }
}
