# KenBot
A twitch bot for monitoring chat.

Responds to users' commands.
Command include:

# Public commands:

<b>!song</b>

    Summary:
        Responds with the name of the song currently playing.
          
<b>!level</b>

    Summary:
       Responds with the level of the streamer in-game. (Will support CS:GO and Dota2)
        
<b>!rank</b>

    Summary:
        Responds with the rank of the streamer in-game. (Will support CS:GO and Dota2)
        
# Private commands (for Mods/Broadcaster):

<b>!automsg [message] [frequency]</b>

    Summary:
        Sets the [message] that is automatically sent every [frequency].
    Arguments:
        [message]:
           -The message to be sent.
        [frequency]:
            -The interval in seconds.
          
<b>!automsg [on/off]</b>

    Summary:
        Enables/disables the automsg feature.
    Arguments:
        [on/off]:
            -Sets the state of the automsg feature. (Can only be 'on' or 'off')
