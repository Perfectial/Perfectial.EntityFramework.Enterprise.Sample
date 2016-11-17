namespace Perfectial.Infrastructure.Identity.Model
{
    public class IdentityMessage
    {
        public virtual string Destination { get; set; }
        public virtual string Subject { get; set; }
        public virtual string Body { get; set; }
    }
}
