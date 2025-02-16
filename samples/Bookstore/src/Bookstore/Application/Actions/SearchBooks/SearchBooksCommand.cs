using OpenDDD.Application;

namespace Bookstore.Application.Actions.SearchBooks
{
    public class SearchBooksCommand : ICommand
    {
        public string TitleFreeText { get; }
        
        public SearchBooksCommand() { }

        public SearchBooksCommand(string titleFreeText)
        {
            TitleFreeText = titleFreeText;
        }
    }
}
