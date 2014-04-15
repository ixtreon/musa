using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ToDo.Properties;

namespace ToDo
{
    public class Task
    {
        public static readonly Task Empty = new Task("Add a task...");


        public readonly TaskData Data;

        public event Action<Task> SomethingChanged = (t) => { };    

        /// <summary>
        /// Gets or sets the TaskEntry associated to this Task. 
        /// </summary>
        public TaskEntry Entry { get; set; }


        public bool IsEmpty
        {
            get
            {
                return this.Equals(Task.Empty);
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                if (Completed || !Data.ExpirationDate.HasValue)
                    return TimeSpan.MaxValue;
                return Data.ExpirationDate.Value.Subtract(Data.CreationDate);
            }
        }

        public TimeSpan RemainingTime
        {
            get
            {
                if (Completed || !Data.ExpirationDate.HasValue)
                    return TimeSpan.MaxValue;
                return Data.ExpirationDate.Value.Subtract(DateTime.Now);
            }
        }

        public TimeSpan ElapsedTime
        {
            get
            {
                return DateTime.Today.Subtract(Data.CreationDate);
            }
        }

        public bool Old
        {
            get
            {
                return Data.CompletionDate.HasValue && DateTime.Now.Subtract(Data.CompletionDate.Value).TotalDays > Settings.Default.taskOldTimeDays;
            }
        }

        public bool Completed
        {
            get { return Data.CompletionDate.HasValue; }
         
            set
            {
                if (value)
                    Data.CompletionDate = DateTime.Today;
                else
                    Data.CompletionDate = null;
                SomethingChanged(this);
            }
        }

        public DateTime? ExpirationDate
        {
            get { return Data.ExpirationDate; }
            set { Data.ExpirationDate = value; SomethingChanged(this); }
        }

        public string Text
        {
            get { return Data.Text; }
            set { Data.Text = value; SomethingChanged(this); }
        }

        public string Description
        {
            get { return Data.Description; }
            set { Data.Description = value; SomethingChanged(this); }
        }

        public DateTime CreationDate
        {
            get { return Data.CreationDate; }
            set { Data.CreationDate = value; SomethingChanged(this); }
        }


        //constructors
        public Task()
        {
            this.Data = new TaskData();
            this.Data.CreationDate = DateTime.Now;
        }

        public Task(string text, string description = "") : this()
        {
            this.Data.Text = text;
            this.Data.Description = description;
        }

        public Task(TaskData t)
        {
            this.Data = t;
        }


        public virtual void OnSomethingChanged()
        {
            SomethingChanged(this);
        }
    }
}
