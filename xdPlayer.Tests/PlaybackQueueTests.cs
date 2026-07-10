using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Playback;

namespace xdPlayer.Tests;

public class PlaybackQueueTests
{
    private static Track MakeTrack(int id) => new Track { Id = id, Title = $"Track {id}" };

    private static PlaybackQueue MakeQueueWithTracks(int count)
    {
        var queue = new PlaybackQueue();
        for (int i = 1; i <= count; i++)
            queue.Add(MakeTrack(i));
        return queue;
    }

    [Fact]
    public void Next_MovesToNextTrack_WhenNotAtEnd()
    {
        var queue = MakeQueueWithTracks(3);

        var next = queue.Next();

        Assert.NotNull(next);
        Assert.Equal(2, next!.Id);
        Assert.Equal(1, queue.CurrentIndex);
    }

    [Fact]
    public void Next_ReturnsNull_WhenAtEndAndRepeatNone()
    {
        var queue = MakeQueueWithTracks(3);
        queue.RepeatMode = RepeatMode.None;
        queue.SetIndex(2);

        var next = queue.Next();

        Assert.Null(next);
    }

    [Fact]
    public void Next_WrapsToStart_WhenAtEndAndRepeatAll()
    {
        var queue = MakeQueueWithTracks(3);
        queue.RepeatMode = RepeatMode.All;
        queue.SetIndex(2);

        var next = queue.Next();

        Assert.NotNull(next);
        Assert.Equal(1, next!.Id);
        Assert.Equal(0, queue.CurrentIndex);
    }

    [Fact]
    public void Next_ReturnsSameTrack_WhenRepeatOne()
    {
        var queue = MakeQueueWithTracks(3);
        queue.RepeatMode = RepeatMode.One;
        queue.SetIndex(1);

        var next = queue.Next();

        Assert.NotNull(next);
        Assert.Equal(2, next!.Id);
        Assert.Equal(1, queue.CurrentIndex);
    }

    [Fact]
    public void Next_ReturnsNull_WhenQueueIsEmpty()
    {
        var queue = new PlaybackQueue();

        var next = queue.Next();

        Assert.Null(next);
    }

    [Fact]
    public void Previous_DoesNotGoBelowZero()
    {
        var queue = MakeQueueWithTracks(3);
        queue.SetIndex(0);

        var previous = queue.Previous();

        Assert.NotNull(previous);
        Assert.Equal(1, previous!.Id);
        Assert.Equal(0, queue.CurrentIndex);
    }

    [Fact]
    public void Previous_MovesBack_WhenNotAtStart()
    {
        var queue = MakeQueueWithTracks(3);
        queue.SetIndex(2);

        var previous = queue.Previous();

        Assert.NotNull(previous);
        Assert.Equal(2, previous!.Id);
        Assert.Equal(1, queue.CurrentIndex);
    }

    [Fact]
    public void Shuffle_ResetsCurrentIndexToZero()
    {
        var queue = MakeQueueWithTracks(5);
        queue.SetIndex(3);

        queue.Shuffle();

        Assert.Equal(0, queue.CurrentIndex);
    }

    [Fact]
    public void Shuffle_PreservesAllOriginalTracks()
    {
        var queue = MakeQueueWithTracks(5);
        var originalIds = queue.Tracks.Select(t => t.Id).OrderBy(id => id).ToList();

        queue.Shuffle();

        var shuffledIds = queue.Tracks.Select(t => t.Id).OrderBy(id => id).ToList();
        Assert.Equal(originalIds, shuffledIds);
    }

    [Fact]
    public void Remove_TrackBeforeCurrentIndex_DecrementsCurrentIndex()
    {
        var queue = MakeQueueWithTracks(3);
        queue.SetIndex(2);
        var trackToRemove = queue.Tracks[0];

        queue.Remove(trackToRemove);

        Assert.Equal(1, queue.CurrentIndex);
    }

    [Fact]
    public void Remove_CurrentTrack_WhenItIsLast_MovesIndexBack()
    {
        var queue = MakeQueueWithTracks(3);
        queue.SetIndex(2);
        var trackToRemove = queue.Tracks[2];

        queue.Remove(trackToRemove);

        Assert.Equal(1, queue.CurrentIndex);
        Assert.Equal(2, queue.Tracks.Count);
    }

    [Fact]
    public void Clear_ResetsQueueAndIndex()
    {
        var queue = MakeQueueWithTracks(3);
        queue.SetIndex(2);

        queue.Clear();

        Assert.Empty(queue.Tracks);
        Assert.Equal(0, queue.CurrentIndex);
        Assert.Null(queue.CurrentTrack);
    }

    [Fact]
    public void SetIndex_IgnoresOutOfRangeValues()
    {
        var queue = MakeQueueWithTracks(3);
        queue.SetIndex(1);

        queue.SetIndex(99);

        Assert.Equal(1, queue.CurrentIndex);
    }
}