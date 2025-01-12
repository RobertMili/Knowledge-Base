package entity;

import jakarta.persistence.*;

@Entity
@NamedQuery(query = "SELECT b from BokEntity b WHERE b.bokForfattare = :forfattare", name = "bokForfattareQuery")
@NamedQuery(query = "SELECT b from BokEntity b", name = "bokQuery")
@Table(name = "bok", schema = "jpaBokVer1", catalog = "")
public class BokEntity {
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Id
    @Column(name = "bokId", nullable = false)
    private int bokId;
    @Basic
    @Column(name = "bokTitel", nullable = true, length = 50)
    private String bokTitel;
    @Basic
    @Column(name = "bokForfattare", nullable = true, length = 50)
    private String bokForfattare;
    @Basic
    @Column(name = "bokBeskrivn", nullable = true, length = 50)
    private String bokBeskrivn;
    @Basic
    @Column(name = "bokIsbn", nullable = true, length = 50)
    private String bokIsbn;
    @Basic
    @Column(name = "bokPris", nullable = true)
    private Integer bokPris;

    public int getBokId() {
        return bokId;
    }

    public void setBokId(int bokId) {
        this.bokId = bokId;
    }

    public String getBokTitel() {
        return bokTitel;
    }

    public void setBokTitel(String bokTitel) {
        this.bokTitel = bokTitel;
    }

    public String getBokForfattare() {
        return bokForfattare;
    }

    public void setBokForfattare(String bokForfattare) {
        this.bokForfattare = bokForfattare;
    }

    public String getBokBeskrivn() {
        return bokBeskrivn;
    }

    public void setBokBeskrivn(String bokBeskrivn) {
        this.bokBeskrivn = bokBeskrivn;
    }

    public String getBokIsbn() {
        return bokIsbn;
    }

    public void setBokIsbn(String bokIsbn) {
        this.bokIsbn = bokIsbn;
    }

    public Integer getBokPris() {
        return bokPris;
    }

    public void setBokPris(Integer bokPris) {
        this.bokPris = bokPris;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        BokEntity bokEntity = (BokEntity) o;

        if (bokId != bokEntity.bokId) return false;
        if (bokTitel != null ? !bokTitel.equals(bokEntity.bokTitel) : bokEntity.bokTitel != null) return false;
        if (bokForfattare != null ? !bokForfattare.equals(bokEntity.bokForfattare) : bokEntity.bokForfattare != null)
            return false;
        if (bokBeskrivn != null ? !bokBeskrivn.equals(bokEntity.bokBeskrivn) : bokEntity.bokBeskrivn != null)
            return false;
        if (bokIsbn != null ? !bokIsbn.equals(bokEntity.bokIsbn) : bokEntity.bokIsbn != null) return false;
        if (bokPris != null ? !bokPris.equals(bokEntity.bokPris) : bokEntity.bokPris != null) return false;

        return true;
    }

    @Override
    public int hashCode() {
        int result = bokId;
        result = 31 * result + (bokTitel != null ? bokTitel.hashCode() : 0);
        result = 31 * result + (bokForfattare != null ? bokForfattare.hashCode() : 0);
        result = 31 * result + (bokBeskrivn != null ? bokBeskrivn.hashCode() : 0);
        result = 31 * result + (bokIsbn != null ? bokIsbn.hashCode() : 0);
        result = 31 * result + (bokPris != null ? bokPris.hashCode() : 0);
        return result;
    }
}
