namespace Gamesbakery.WebGUI.Models
{
    public class SingleResponse<T>
    {
        /// <summary>
        /// The requested item.
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// A message describing the result of the operation.
        /// </summary>
        public string Message { get; set; }
    }
}
