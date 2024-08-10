using System;
using System.Collections.Generic;
using TwitchLib.Api.Core.RateLimiter;
public class StringRingBuffer
{
    public string[] _buffer;
    public int _head;
    public int _tail;
    public int _count;
    private int currentIndex;
    private readonly int _capacity;

    public StringRingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity needs to be greater then zero", nameof(capacity));

        _capacity = capacity;
        _buffer = new string[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;
        currentIndex = -1;
    }

    public void Add(string word)
    {
        // % _capacity just means it will never get an error for being over or under the array length as it just wraps it around
        // to find the next index in limits to the _capacity.
        if (_count == _capacity)
        {
            //Buffer is full
            _head = (_head + 1) % _capacity;
        }
        else
        {
            _count++;
        }

        _buffer[_tail] = word;
        _tail = (_tail + 1) % _capacity;
        currentIndex = -1;
    }

    public string GetIndexAbove()
    {
        // if there is nothing above it
        if (_count == 0) { return null; }
        
        if (currentIndex == -1)
        {
            currentIndex = (_tail - 1 + _capacity) % _capacity;
        }
        else
        {
            // Go to the previous index
            currentIndex = (currentIndex - 1 + _capacity) % _capacity;
            if (currentIndex == (_head - 1 + _capacity) % _capacity)
            {
                currentIndex = (_tail -1 + _capacity) % _capacity;
            }
        }

        return _buffer[currentIndex];
    }

    public string GetIndexBelow()
    {
        return string.Empty;
    }
}