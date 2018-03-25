using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IDeliverable.Utils.Core.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Description = Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;

namespace IDeliverable.Utils.Core.Tests
{
    [TestClass]
    public class GroupingProjectionTest
    {
        private ObservableCollection<Happening> mSourceCollection;
        private GroupingProjection<DateTime, DateTime, Happening> mTarget;

        [TestInitialize]
        public void TestInitialize()
        {
            mSourceCollection = new ObservableCollection<Happening>(new Happening[]
            {
                new Happening(new DateTime(2017, 01, 01, 12, 00, 00), "HappeningDay1@12"),
                new Happening(new DateTime(2017, 01, 01, 14, 00, 00), "HappeningDay1@14"),
                new Happening(new DateTime(2017, 01, 01, 16, 00, 00), "HappeningDay1@16"),
                new Happening(new DateTime(2017, 01, 01, 10, 00, 00), "HappeningDay1@10"),
                new Happening(new DateTime(2017, 01, 01, 08, 00, 00), "HiddenDay1"),
                new Happening(new DateTime(2017, 01, 02, 12, 00, 00), "HappeningDay2@12"),
                new Happening(new DateTime(2017, 01, 02, 14, 00, 00), "HappeningDay2@14"),
                new Happening(new DateTime(2017, 01, 02, 16, 00, 00), "HappeningDay2@16"),
                new Happening(new DateTime(2017, 01, 02, 10, 00, 00), "HappeningDay2@10"),
                new Happening(new DateTime(2017, 01, 02, 08, 00, 00), "HiddenDay2")
            });

            mTarget =
                new GroupingProjection<DateTime, DateTime, Happening>(
                    mSourceCollection,
                    item => item.Time.Date,
                    item => item.Time,
                    Comparer<DateTime>.Create((x, y) => x.CompareTo(y)),
                    item => !item.Name.StartsWith("Hidden"));
        }

        #region Constructor

        [TestMethod]
        [Description("Initial grouping is correctly performed.")]
        public void ConstructorTest01()
        {
            Assert.AreEqual(2, mTarget.Count);
            Assert.AreEqual(4, mTarget[0].Items.Count);
            Assert.AreEqual(4, mTarget[1].Items.Count);
            Assert.AreEqual("HappeningDay1@10", mTarget[0].Items[0].Name);
            Assert.AreEqual("HappeningDay1@12", mTarget[0].Items[1].Name);
            Assert.AreEqual("HappeningDay1@14", mTarget[0].Items[2].Name);
            Assert.AreEqual("HappeningDay1@16", mTarget[0].Items[3].Name);
            Assert.AreEqual("HappeningDay2@10", mTarget[1].Items[0].Name);
            Assert.AreEqual("HappeningDay2@12", mTarget[1].Items[1].Name);
            Assert.AreEqual("HappeningDay2@14", mTarget[1].Items[2].Name);
            Assert.AreEqual("HappeningDay2@16", mTarget[1].Items[3].Name);
        }

        #endregion

        [TestMethod]
        [Description("Adding a new item with a new group value creates a new group and raises the appropriate events.")]
        public void ChangeTest01()
        {
            var groupCollectionAddEventRaised = false;
            var newTime = new DateTime(2017, 01, 03, 12, 00, 00);
            var newName = "HappeningDay3@12";

            mTarget.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add
                        && e.NewItems.Count == 1
                        && e.NewItems[0] is GroupingProjection<DateTime, DateTime, Happening>.Group addedGroup
                        && addedGroup.GroupKey == newTime.Date
                        && addedGroup.Items.Count == 1)
                    groupCollectionAddEventRaised = true;
            };

            mSourceCollection.Add(new Happening(newTime, newName));

            Assert.AreEqual(3, mTarget.Count);
            Assert.AreEqual(1, mTarget[2].Items.Count);
            Assert.AreEqual(newTime.Date, mTarget[2].GroupKey);
            Assert.AreEqual(newName, mTarget[2].Items[0].Name);
            Assert.IsTrue(groupCollectionAddEventRaised);
        }

        [TestMethod]
        [Description("Adding a new item to an existing group raises the appropriate events.")]
        public void ChangeTest02()
        {
            var collectionAddEventRaised = false;
            var newTime = new DateTime(2017, 01, 02, 18, 00, 00);
            var newName = "HappeningDay2@18";

            ((INotifyCollectionChanged)mTarget[1].Items).CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add
                        && e.NewItems.Count == 1
                        && e.NewItems[0] is Happening addedHappening
                        && addedHappening.Name == newName)
                    collectionAddEventRaised = true;
            };

            mSourceCollection.Add(new Happening(newTime, newName));

            Assert.AreEqual(2, mTarget.Count);
            Assert.AreEqual(5, mTarget[1].Items.Count);
            Assert.AreEqual(newName, mTarget[1].Items[4].Name);
            Assert.IsTrue(collectionAddEventRaised);
        }

        [TestMethod]
        [Description("Removing an item from a group raises the appropriate events.")]
        public void ChangeTest03()
        {
            var collectionRemoveEventRaised = false;
            var happeningToRemove = mTarget[1].Items.Last();

            ((INotifyCollectionChanged)mTarget[1].Items).CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove
                        && e.OldItems.Count == 1
                        && e.OldItems[0] is Happening removedHappening
                        && removedHappening.Name == happeningToRemove.Name)
                    collectionRemoveEventRaised = true;
            };

            mSourceCollection.Remove(happeningToRemove);

            Assert.AreEqual(2, mTarget.Count);
            Assert.AreEqual(3, mTarget[1].Items.Count);
            Assert.IsTrue(collectionRemoveEventRaised);
        }

        [TestMethod]
        [Description("Removing the last item from a group removes the group and raises the appropriate events.")]
        public void ChangeTest04()
        {
            mSourceCollection.RemoveAt(7);
            mSourceCollection.RemoveAt(6);
            mSourceCollection.RemoveAt(5);

            var groupCollectionRemoveEventRaised = false;
            var happeningToRemove = mTarget[1].Items.Single();

            mTarget.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove
                        && e.OldItems.Count == 1
                        && e.OldItems[0] is GroupingProjection<DateTime, DateTime, Happening>.Group removedGroup
                        && removedGroup.GroupKey == happeningToRemove.Time.Date)
                    groupCollectionRemoveEventRaised = true;
            };

            mSourceCollection.Remove(happeningToRemove);

            Assert.AreEqual(1, mTarget.Count);
            Assert.IsTrue(groupCollectionRemoveEventRaised);
        }

        [TestMethod]
        [Description("Changing the group value of an item to a new group value moves it to a new group and raises the appropriate events.")]
        public void ChangeTest05()
        {
            var collectionRemoveEventRaised = false;
            var groupCollectionAddEventRaised = false;
            var happeningToMove = mTarget[1].Items.Last();

            var newTime = new DateTime(2017, 01, 03, 12, 00, 00);

            ((INotifyCollectionChanged)mTarget[1].Items).CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove
                        && e.OldItems.Count == 1
                        && e.OldItems[0] is Happening removedHappening
                        && removedHappening.Name == happeningToMove.Name)
                    collectionRemoveEventRaised = true;
            };

            mTarget.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add
                        && e.NewItems.Count == 1
                        && e.NewItems[0] is GroupingProjection<DateTime, DateTime, Happening>.Group addedGroup
                        && addedGroup.GroupKey == newTime.Date
                        && addedGroup.Items.Count == 1)
                    groupCollectionAddEventRaised = true;
            };

            happeningToMove.Time = newTime;

            Assert.AreEqual(3, mTarget.Count);
            Assert.AreEqual(4, mTarget[0].Items.Count);
            Assert.AreEqual(3, mTarget[1].Items.Count);
            Assert.AreEqual(1, mTarget[2].Items.Count);
            Assert.AreEqual(newTime.Date, mTarget[2].GroupKey);
            Assert.IsTrue(collectionRemoveEventRaised);
            Assert.IsTrue(groupCollectionAddEventRaised);
        }

        [TestMethod]
        [Description("Changing the group value of an item moves it to another group and raises the appropriate events.")]
        public void ChangeTest06()
        {
            var collectionRemoveEventRaised = false;
            var collectionAddEventRaised = false;
            var happeningToMove = mTarget[0].Items.Last();

            var newTime = new DateTime(2017, 01, 02, 18, 00, 00);

            ((INotifyCollectionChanged)mTarget[0].Items).CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove
                        && e.OldItems.Count == 1
                        && e.OldItems[0] is Happening removedHappening
                        && removedHappening.Name == happeningToMove.Name)
                    collectionRemoveEventRaised = true;
            };

            ((INotifyCollectionChanged)mTarget[1].Items).CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add
                        && e.NewItems.Count == 1
                        && e.NewItems[0] is Happening addedHappening
                        && addedHappening.Name == happeningToMove.Name)
                    collectionAddEventRaised = true;
            };

            happeningToMove.Time = newTime;

            Assert.AreEqual(2, mTarget.Count);
            Assert.AreEqual(3, mTarget[0].Items.Count);
            Assert.AreEqual(5, mTarget[1].Items.Count);
            Assert.IsTrue(collectionRemoveEventRaised);
            Assert.IsTrue(collectionAddEventRaised);
        }

        [TestMethod]
        [Description("Changing the order key of an item moves it within its group and raises the appropriate events.")]
        public void ChangeTest07()
        {
            var collectionMoveEventRaised = false;
            var happeningToMove = mSourceCollection[2];

            var newTime = new DateTime(2017, 01, 01, 11, 00, 00);

            ((INotifyCollectionChanged)mTarget[0].Items).CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Move && e.OldStartingIndex == 3 && e.NewStartingIndex == 1)
                    collectionMoveEventRaised = true;
            };

            Assert.AreEqual(3, mTarget[0].Items.IndexOf(happeningToMove));

            happeningToMove.Time = newTime;

            Assert.AreEqual(1, mTarget[0].Items.IndexOf(happeningToMove));
            Assert.IsTrue(collectionMoveEventRaised);
        }

        [TestMethod]
        [Description("Adding a new item to an existing group places the item at the correct location in the group with respect to the ordering.")]
        public void ChangeTest08()
        {
            var newTime = new DateTime(2017, 01, 01, 13, 00, 00);
            var newName = "HappeningDay1@13";
            var newHappening = new Happening(newTime, newName);

            mSourceCollection.Add(newHappening);

            Assert.AreEqual(2, mTarget[0].Items.IndexOf(newHappening));
        }

        [TestMethod]
        [Description("Adding a new item with a new group key creates the new group at the correct location with respect to the ordering.")]
        public void ChangeTest09()
        {
            var newTime = new DateTime(2016, 12, 31, 12, 00, 00);
            var newName = "HappeningDay-1@12";

            mSourceCollection.Add(new Happening(newTime, newName));

            Assert.AreEqual(newTime.Date, mTarget[0].GroupKey);
        }

        [TestMethod]
        [Description("Changing an item so that it passes the filter predicate adds it at the correct location in the correct group and raises the appropriate events.")]
        public void ChangeTest10()
        {
            var collectionAddEventRaised = false;
            var happeningToChange = mSourceCollection[4];

            var newName = "HappeningDay1@08";

            ((INotifyCollectionChanged)mTarget[0].Items).CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add
                        && e.NewItems.Count == 1
                        && e.NewItems[0] is Happening addedHappening
                        && addedHappening.Name == newName)
                    collectionAddEventRaised = true;
            };

            Assert.IsFalse(mTarget[0].Items.Contains(happeningToChange));

            happeningToChange.Name = newName;

            Assert.AreEqual(0, mTarget[0].Items.IndexOf(happeningToChange));
            Assert.IsTrue(collectionAddEventRaised);
        }

        [TestMethod]
        [Description("Changing an item so that it no longer passes the filter predicate removes it from its group and raises the appropriate events.")]
        public void ChangeTest11()
        {
            var collectionRemoveEventRaised = false;
            var happeningToChange = mSourceCollection[0];

            var newName = "HiddenDay1@12";

            ((INotifyCollectionChanged)mTarget[0].Items).CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove
                        && e.OldItems.Count == 1
                        && e.OldItems[0] is Happening removedHappening
                        && removedHappening.Name == newName)
                    collectionRemoveEventRaised = true;
            };

            Assert.AreEqual(1, mTarget[0].Items.IndexOf(happeningToChange));

            happeningToChange.Name = newName;

            Assert.IsFalse(mTarget[0].Items.Contains(happeningToChange));
            Assert.IsTrue(collectionRemoveEventRaised);
        }

        [TestMethod]
        [Description("Adding a new item with a new group value does not raise any events within a BeginUpdate/EndUpdate block, but raises a collection reset event on EndUpdate.")]
        public void ChangeTest12()
        {
            var groupCollectionAnyEventRaised = false;
            var groupCollectionAddEventRaised = false;
            var groupCollectionResetEventRaised = false;
            var newTime = new DateTime(2017, 01, 03, 12, 00, 00);
            var newName = "HappeningDay3@12";

            mTarget.CollectionChanged += (sender, e) =>
            {
                groupCollectionAnyEventRaised = true;
                groupCollectionAddEventRaised = e.Action == NotifyCollectionChangedAction.Add;
                groupCollectionResetEventRaised = e.Action == NotifyCollectionChangedAction.Reset;
            };

            mTarget.BeginUpdate();

            mSourceCollection.Add(new Happening(newTime, newName));

            Assert.IsFalse(groupCollectionAnyEventRaised);

            mTarget.EndUpdate();

            Assert.IsTrue(groupCollectionAnyEventRaised);
            Assert.IsFalse(groupCollectionAddEventRaised);
            Assert.IsTrue(groupCollectionResetEventRaised);
        }

        private class Happening : INotifyPropertyChanged
        {
            public Happening(DateTime time, string name)
            {
                mTime = time;
                mName = name;
            }

            private DateTime mTime;
            private string mName;

            public DateTime Time
            {
                get => mTime;
                set
                {
                    if (value != mTime)
                    {
                        mTime = value;
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
