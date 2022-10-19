<template>
    <h1>Collaborative Word Guess</h1>
    <img src="example.png" id="example" />
    <p>
        This is a collaborative version of a word-guessing game. Either everyone wins, or you are all losers.
    <p>
    </p>
        There is a fixed duration for each guess, and the most popular word is automatically selected when the time runs out for each guess.
    </p>
    <p>
        Options to play:
        <ul class="browser-default">
            <li>Join an existing game, via a URL from the game organiser</li>
            <li><router-link :to="{ name: 'Admin' }" id="start">Start a new game</router-link></li>
        </ul>
        No login is required 👍 Your metadata is not tracked or stored 👍
    </p>
</template>

<style scoped>
    #example {
        width: 200px;
        float: right;
    }
    #start {
        font-size: 1.4em;
    }
</style>
