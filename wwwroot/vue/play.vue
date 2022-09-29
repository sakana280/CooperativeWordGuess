<template>
    <h1>Collaborative Word Guess</h1>
    <p>The most popular word is automatically selected when the time runs out for each guess.</p>
    <game :public-token="$route.params.publicToken" :guess-word="word" @playing="onPlaying" @guess="onGuess" @finished="onFinished" />
    <keyboard v-if="isPlaying" v-model="word" :max-length="wordLength" :letter-states="letterStates" />
</template>

<style scoped>
    h1, p {
        text-align: center;
    }
</style>

<script>
    import game from './game.vue'
    import keyboard from './keyboard.vue'

    export default {
        data() {
            return {
                word: null,
                wordLength: null,
                letterStates: [],
                isPlaying: false,
            }
        },

        components: {
            game,
            keyboard,
        },

        methods: {
            onPlaying(wordLength) {
                this.wordLength = wordLength
                this.isPlaying = true
            },

            onGuess(letterStates) {
                this.word = null
                this.letterStates = letterStates
            },

            onFinished() {
                this.isPlaying = false
            },
        },
    }

</script>
