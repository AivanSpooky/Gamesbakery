namespace Gamesbakery.WebGUI.Models
{
    public class SingleResponse<T>
    {
        /// <summary>
        /// Gets or sets the requested item.
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result of the operation.
        /// </summary>
        public string Message { get; set; }
    }
}
