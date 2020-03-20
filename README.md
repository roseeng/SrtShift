# SrtShift
A simple commandline tool that can shift the timestamps in an .srt subtitle file

Usage: SrtShift <filename> <firstSound> <firstText> { <lastSound> <lastText> }

where firstSound is a line you can hear as early as possible in the film, and
firstText is the timestamp of the corresponding textline.

This is normally all that's needed, and all timestamps will be shifted by 
an equal amount,but if the speed of the film does not match the subtitles, 
you can add a sound/text pair from the end of the film, and SrtShift will
calculate a rate factor so that lastText matches lastSound.

Output file will be "<filenameWithoutExtension>.out.srt"
