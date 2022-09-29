<template>
    <div v-for="row of layout" class="keyboard">
        <div v-for="letter of row" :class="`guess-letter-${(letterStates[letter] || 'Initial').toLowerCase()}`" @click="onKey(letter === '<' ? 'Backspace' : letter)">
            {{letter === '<' ? 'del' : letter}}
        </div>
    </div>
</template>

<style scoped>
    .keyboard {
        text-align: center;
        font-size: 0.8em;
        white-space: nowrap;
    }
    .keyboard div {
        display: inline-block;
        border-radius: 5px;
        vertical-align: middle;
        font-size: 2em;
        min-width: 1.5em;
        height: 2em;
        line-height: 2em;
        margin: 2px 2px;
        padding: 2px 2px;
        cursor: pointer;
    }
</style>

<script>

    export default {
        props: {
            modelValue: '',
            maxLength: Number,
            letterStates: {}
        },

        emits: ['update:modelValue'],

        data() {
            return {
                layout: ['QWERTYUIOP', 'ASDFGHJKL', 'ZXCVBNM<'],
                word: '',
            }
        },

        methods: {
            onKeyDown(event) {
                if (!event.repeat) {
                    if (/^[a-zA-Z]$/.test(event.key)) {
                        this.onKey(event.key.toUpperCase())
                    } else if (event.code === 'Backspace') {
                        this.onKey('Backspace')
                    }
                }
                event.preventDefault()
            },

            onKey(key) {
                var word = this.word || '' // handle null bindings

                if (key === 'Backspace') {
                    if (word.length > 0) {
                        word = word.substring(0, word.length - 1)
                    }
                } else if (word.length < this.maxLength) {
                    word += key.toUpperCase()
                }
                this.word = word
                this.$emit('update:modelValue', word)
            },
       },

        expose: [],

        watch: {
            modelValue() {
                this.word = this.modelValue
            },
        },

        mounted() {
            window.addEventListener('keydown', this.onKeyDown)
        },

        unmounted() {
            window.addEventListener('keydown', this.onKeyDown)
        },
    }
</script>
