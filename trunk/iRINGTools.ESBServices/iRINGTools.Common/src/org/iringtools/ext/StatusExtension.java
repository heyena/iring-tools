package org.iringtools.ext;

import org.iringtools.common.response.Status;
import org.iringtools.library.StatusLevel;

public class StatusExtension extends Status {
    protected StatusLevel level;
    
	public StatusLevel getLevel() {
        return level;
    }
    public void setLevel(StatusLevel value) {
        this.level = value;
    }
}
