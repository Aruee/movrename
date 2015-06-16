This is a small project that renames movie files according to the date they were
taken on.

I shoot a lot of movies using my DSLR, mostly of my kids. Unlike the pictures,
the resulting files do not contain some kind of metadata that doesn't get lost
when the file is copied to some obscure outdated file system that messes up your
file timestamps. So, I wrote this little program. So far, this is just for me so
you better use it at your own risk: it looks for .mov files in a certain folder,
analyses the name and finds adjacent JPG files to retrieve the date from. If
that worked, the file is renamed to contain the date in the filename. Because
I have yet to encounter a file system that renames your files at will.

This works for my Canon.

That's really all.


