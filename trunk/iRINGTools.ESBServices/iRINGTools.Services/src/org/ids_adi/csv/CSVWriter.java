package org.ids_adi.csv;

import java.io.*;

public class CSVWriter {
    private final Writer writer;
    private boolean inCell;
    private boolean inLine;
    private boolean quoted;
    private String quote = "\"";
    private String comma = ",";
    private String eol = "\r\n";
    private String quoteSequences[];
    private int column;
    private int row;
    private boolean flushLine;
    private boolean flushEnd;

    public CSVWriter(Writer writer, boolean flushLine, boolean flushEnd) {
    	this.writer = writer;
	this.flushLine = flushLine;
	this.flushEnd = flushEnd;
    }

    public CSVWriter(Writer writer) {
    	this (writer, false, false);
    }

    public void beginDocument() {
	if (quoteSequences != null) {
	    throw new IllegalStateException();
	}

	quoteSequences = new String[] { quote, comma, eol };
    }

    public void endDocument() {
	if (quoteSequences == null) {
	    throw new IllegalStateException();
	}

    	quoteSequences = null;

	if (flushEnd) {
	    flush();
	}
    }

    protected void flush() {
	try {
	    writer.flush();
	}
	catch (IOException e) {
	    throw new RuntimeException(e);
	}
    }

    protected void send(String text, int offset, int length) {
	try {
	    writer.write(text, offset, length);
	}
	catch (IOException e) {
	    throw new RuntimeException(e);
	}
    }

    protected void send(String text) {
    	if (text != null) {
	    final int length = text.length();

	    if (length > 0) {
		send(text, 0, length);
	    }
	}
    }

    public void contents(String text) {
    	if (quoted) {
	    final int size = text.length();
	    int offset = 0;
	    int index;

	    while ((index = text.indexOf(quote, offset)) >= 0) {
	    	if (index > offset) {
		    send(text, offset, index - offset);
		}

		send(quote);
		send(quote);

		offset = index + quote.length();
	    }

	    if (offset < size) {
		send(text, offset, size - offset);
	    }
	}
	else {
	    if (needsQuotes(text)) {
		throw new IllegalStateException();
	    }

	    send(text);
	}
    }

    public void beginCell(boolean quoted) {
    	if (!inLine || inCell) {
	    throw new IllegalStateException();
	}

	inCell = true;
	this.quoted = quoted;

	if (column != 0) {
	    send(comma);
	}

	if (quoted) {
	    send(quote);
	}
    }

    public void endCell() {
    	if (!inCell) {
	    throw new IllegalStateException();
	}

	if (quoted) {
	    send(quote);
	}

	inCell = false;
	column++;
    }

    public void beginLine() {
    	if (inLine) {
	    throw new IllegalStateException();
	}

	inLine = true;
    }

    public void endLine() {
    	if (!inLine || inCell) {
	    throw new IllegalStateException();
	}

	inLine = false;

	row++;
	column = 0;

	send(eol);

	if (flushLine) {
	    flush();
	}
    }

    public boolean needsQuotes(String text) {
	boolean needs = false;

	if (text == null) {
	    // nothing
	}
    	else {
	    for (int i = 0; !needs && (i < quoteSequences.length); i++) {
	    	needs = (text.indexOf(quoteSequences[i]) >= 0);
	    }
	}

	return (needs);
    }

    public void sendCell(String text, boolean quoted) {
    	beginCell(quoted);

	contents(text);

	endCell();
    }

    public void sendCell(String text) {
    	sendCell(text, needsQuotes(text));
    }
}

