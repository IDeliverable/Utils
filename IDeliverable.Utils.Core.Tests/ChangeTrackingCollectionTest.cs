using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IDeliverable.Utils.Core.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Description = Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute;

namespace IDeliverable.Utils.Core.Tests
{
    [TestClass]
    public class ChangeTrackingCollectionTest
    {
        [TestMethod]
        [Description("Adding an item with an ID that already exists throws.")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddTest01()
        {
            var duplicateId = Guid.NewGuid();

            var sourceCollection = new ObservableCollection<Thing>(new Thing[]
            {
                new Thing(duplicateId, "Thing1"),
                new Thing(Guid.NewGuid(), "Thing2"),
                new Thing(Guid.NewGuid(), "Thing3")
            });

            var target = new ChangeTrackingCollection<Thing>(sourceCollection)
            {
                new Thing(duplicateId, "Thing4")
            };
        }

        [TestMethod]
        [Description("Setting an item with an ID that already exists throws.")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetTest01()
        {
            var duplicateId = Guid.NewGuid();

            var sourceCollection = new ObservableCollection<Thing>(new Thing[]
            {
                new Thing(duplicateId, "Thing1"),
                new Thing(Guid.NewGuid(), "Thing2"),
                new Thing(Guid.NewGuid(), "Thing3")
            });

            var target = new ChangeTrackingCollection<Thing>(sourceCollection)
            {
                [2] = new Thing(duplicateId, "Thing4")
            };
        }

        [TestMethod]
        [Description("Replacing an item with another item with the same ID doesn't throw.")]
        public void SetTest02()
        {
            var duplicateId = Guid.NewGuid();

            var sourceCollection = new ObservableCollection<Thing>(new Thing[]
            {
                new Thing(duplicateId, "Thing1"),
                new Thing(Guid.NewGuid(), "Thing2"),
                new Thing(Guid.NewGuid(), "Thing3")
            });

            var target = new ChangeTrackingCollection<Thing>(sourceCollection)
            {
                [0] = new Thing(duplicateId, "Thing4")
            };

            Assert.AreEqual("Thing4", target[0].Name);
        }

        private class Thing : INotifyPropertyChanged, IUniqueId
        {
            public Thing(Guid id, string name)
            {
                mId = id;
                mName = name;
            }

            private Guid mId;
            private string mName;

            public Guid UniqueId => mId;

            public Guid Id
            {
                get => mId;
                set
                {
                    if (value != mId)
                    {
                        mId = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string Name
            {
                get => mName;
                set
                {
                    if (value != mName)
                    {
                        mName = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
