using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace connectBase.Entities
{
    public class User
    {
        /// <summary>
        /// Büro+ username
        /// </summary>
        /// <example>username</example>
        [Required]
        public string Username { get; set; }
        /// <summary>
        /// Büro+ password
        /// </summary>
        /// <example>password</example>
        [Required]
        public string Password { get; set; }

        [IgnoreDataMember]
        public string Mandant { get; set; }

        public User() { }
        public User(string name, string password, string mandant)
        {
            this.Username = name;
            this.Password = password;
            this.Mandant = mandant;
        }
    }
}
